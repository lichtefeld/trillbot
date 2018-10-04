using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        private static List<racer> racers = new List<racer>();
        private static Stack<Card> cards = new Stack<Card>();
        private bool endGame = false;
        private static int position = 0;
        private Dictionary<int, Tuple<int,int>> remedy_to_hazards = new Dictionary<int, Tuple<int,int>> {
            {0,new Tuple<int, int>(5,6)},
            {1,new Tuple<int,int>(8,9)},
            {2,new Tuple<int,int>(10,11)}
        };
        public struct pair {
            int item1;
            int item2;
        }

        [Command("startgame")]
        public async Task startGame() {
            await dealCards(); //Deal cards to all racers
            await shuffleRacers(); //Randomize Turn Order
            var guild = Context.Client.GetGuild (Context.Guild.Id);
            var user = guild.GetUser (Context.Client.CurrentUser.Id);
            await Context.Client.SetStatusAsync (UserStatus.Online);
            await Context.Client.SetGameAsync ("The 86th Trilliant Grand Prix", null, StreamType.NotStreaming);
            await ReplyAsync("Game Started");
            await nextTurn();
        }

        [Command("joingame")]
        public async Task joinGame() {
            racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
                return;
            } else {
                foreach (racer r in racers) {
                    if(r.ID == racer.ID) {
                        await ReplyAsync("You have already joined the game!");
                        return;
                    }
                }
                racers.Add(racer);
            }

            await ReplyAsync("You have joined the game");
        }
        private async Task dealCards()
        {
            cards = this.generateDeck();
            foreach(racer r in racers) {
                for(int i = 0; i < 8; i++) {
                    if(cards.Count == 0) { cards = this.generateDeck(); }
                    r.cards.Add(cards.Pop());
                }
                racer.update_racer(r);
            }

            await ReplyAsync("Cards Dealt");
        }

        [Command("playcard")]
        public async Task playCardAsync(int i, int racerID = -1) {
            racer r = racers[position];
            if(r.player_discord_id != Context.Message.Author.Id) {
                await ReplyAsync("It's not your turn!");
                return;
            }
            if (i < 1 && i > 8) {
                await ReplyAsync("You only have 8 cards in your hand! Provide a number 1-8.");
                return;
            }
            Card c = r.cards[--i];
            switch(c.type) { 
                case "Movement":
                    if(!r.canMove) {
                        await ReplyAsync("You currently can't move. Try solving a hazard!");
                        return;
                    }
                    if(c.value > 2 && r.maxMove2) {
                        await ReplyAsync("You currently can't move more than 2 spaces. Try solving a hazard!");
                        return;
                    }
                    switch(c.value) {
                        case 8:
                            if (racerID <= -1 || racerID > 1) {
                                await ReplyAsync("You didn't indicate which version of this card to use.");
                                return;
                            }
                            else if (racerID == 1) {
                                //PASTE DOGE SYSTEM
                            } else if (racerID == 0) {
                                r.distance += c.value;
                                if (r.distance > 24) {
                                    r.distance -= c.value;
                                    await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                    return;
                                }
                                await ReplyAsync(r.name + " played a " + c.title + " spaces. They are now at a distance of " + r.distance + " units." );
                            }
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            r.distance += c.value;
                            if (r.distance > 24) {
                                r.distance -= c.value;
                                await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            await ReplyAsync(r.name + " played a " + c.title + " spaces. They are now at a distance of " + r.distance + " units." );
                            break;
                        default:
                            await ReplyAsync("Um boss, something went wrong. Let's try again!");
                            return;
                    }
                    break;
                case "Hazard":
                    switch(c.value) {
                        case 0:

                        break;
                        case 1:
                            string ram = r.name + " takes the ram action against ";
                            r.distance += 3;
                            if (r.distance > 24) {
                                r.distance -= 3;
                                await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            racers.Where(e=>e.distance == r.distance+1).ToList().ForEach(e => {
                                e.canMove = false;
                                e.hazards.Add(new Tuple<int, int>(c.ID,0));
                                ram += e.name + " ";
                            });
                            await ReplyAsync(ram);
                        break;
                        case 2:
                            r.crash = true;

                        break;
                    }
                    break;
                case "THazard":
                    if(racerID == -1) {
                        await ReplyAsync("You didn't provide a racer ID to target! Try again.");
                        return;
                    }
                    break;
                case "Remedy":

                    break;
                default:
                    await ReplyAsync("Um boss, something went wrong. Let's try again!");
                    return;
            }
            //Handle Survival Checks
            await SurvivalChecks(r);
            //IF Entire Turn Completed Successfully
            await endOfTurnLogic(r, i);
        }

        [Command("reset")] //Reset the game to initial state
        public async Task doReset() {
            cards = new Stack<Card>();
            racers.ForEach(e=> {
                e.reset();
                racer.replace_racer(e);
            });
            racers = new List<racer>();
            position = 0;
            await Context.Client.SetGameAsync(null, null, StreamType.NotStreaming);
            await ReplyAsync("Game Reset");
        }

        [Command("turn")]
        public async Task whosTurnAsync() { //Need to remind a person its there turn?
            var usr = Context.Guild.GetUser(racers[position].player_discord_id);
            if (usr == null ) {
                await ReplyAsync("Uhh boss, something went wrong.");
            }
            await ReplyAsync("Hey " + usr.Mention + " It's your turn now!");
        }

        private async Task endOfTurnLogic(racer r, int i) { //Handle All Logic for Transitioning an End of Turn
            r.cards.RemoveAt(i);
            if(cards.Count == 0 ) cards = generateDeck();
            r.cards.Add(cards.Pop());
            position++;
            racer.update_racer(r);
            if(position == racers.Count) {
                await endOfTurn(); //Handle Passive Movement
                racer winner = checkWinner();
                if(winner != null) {
                    endGame = true;
                    SocketGuildUser usr = Context.Guild.Users.FirstOrDefault(e=>e.Id == winner.player_discord_id);
                    await ReplyAsync(usr + ", you have won the race!");
                    return;
                }
                position -= racers.Count;
            }
            await displayCurrentBoard();
            await nextTurn();
        }

        private async Task SurvivalChecks(racer r) { //Need to Finish Checking all Systems
            if(r.sab && r.hazards.Count > 1) {
                r.stillIn = false;
                await ReplyAsync(r.name + " subcumbs to Sabotage!");
            }
            foreach(Tuple<int,int> t in r.hazards) {
               // t.Item2++;
                if(t.Item2 > 2) {
                    r.stillIn = false;
                    await ReplyAsync(r.name + " subcumbs to " + Card.get_card().FirstOrDefault(e=>e.ID == t.Item1).title + "!");
                }
            }
        }

        private async Task nextTurn() { //DM the next turn person
            var usr = Context.Guild.GetUser(racers[position].player_discord_id);
            if (usr == null ) {
                await ReplyAsync("Uhh boss, something went wrong.");
            }
            string output = "**Your Current Hand**" + System.Environment.NewLine;
            if (racers[position].cards.Count == 0) { 
                await ReplyAsync("Hold up, you don't have any cards. The game must not have started yet.");
            } else {
                for(int i = 0; i < racers[position].cards.Count; i++) {
                    output += "#" + (i+1) + ": " + racers[position].cards[i].ToString() + System.Environment.NewLine;
                }
                await usr.SendMessageAsync(output);
            }
            await Context.Channel.SendMessageAsync(racers[position].name + " has the next turn.");
        }

        private async Task displayCurrentBoard() {
            List<string> str = new List<string>();
            str.Add("**Leaderboard!**");
            str.Add("```");
            str.Add("Distance | Racer Name (ID) | Sponsor");
            var listRacer = racers.OrderBy(e=> e.distance).ToList();
            listRacer.Reverse();
            listRacer.ForEach(e=> str.Add(e.distance + " | " + e.nameID() + " | " + e.faction));
            str.Add("```");
            string ouput_string = string.Join(System.Environment.NewLine, str);

            var channel = Context.Guild.GetTextChannel(Context.Guild.Channels.FirstOrDefault(e=>e.Name == "leaderboard").Id);
            var messages = await this.Context.Channel.GetMessagesAsync(1).Flatten();
            await channel.DeleteMessagesAsync(messages);
            await channel.SendMessageAsync(ouput_string);
             
        }

        private async Task endOfTurn() {
            foreach (racer r in racers ) {
                r.distance++;
                racer.update_racer(r);
            }
            await ReplyAsync("Passive Movement Applied");
        }

        private racer checkWinner() {
            foreach(racer r in racers) {
                if (r.distance == 24) {
                    return r;
                }
            }
            return null;
        }

        private async Task shuffleRacers() {
            List<racer> temp = new List<racer>();

            while (racers.Count > 0) {
                int num = trillbot.Program.rand.Next(0,racers.Count);
                temp.Add(racers[num]);
                racers.RemoveAt(num);
            }

            racers = temp;
            await ReplyAsync("Turn Order Shuffled");
        }
        private async Task doWorkAsyncInfiniteLoop() {
            while(true) {
                if(endGame) {
                    break;
                }
                await Task.Delay(200);
            }
        }

        private Stack<Card> generateDeck() {
            return shuffleDeck(freshDeck());
        }

        private List<Card> freshDeck() {
            List<Card> c = new List<Card>();
            List<Card> temp = trillbot.Classes.Card.get_card();

            foreach (Card c1 in temp) {
                for(int i = 0; i < c1.count; i++) {
                    c.Add(c1);
                }
            }

            return c;
        }

        private Stack<Card> shuffleDeck(List<Card> c) {
            Stack<Card> s = new Stack<Card>();

            while (c.Count > 0) {
                int num = trillbot.Program.rand.Next(0,c.Count);
                s.Push(c[num]);
                c.RemoveAt(num);
            }

            return s;
        }

    }
}