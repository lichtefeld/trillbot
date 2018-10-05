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
        private static int round = 1;
        private Dictionary<int, Tuple<int,int>> remedy_to_hazards = new Dictionary<int, Tuple<int,int>> {
            {0,new Tuple<int, int>(5,6)},
            {1,new Tuple<int,int>(8,9)},
            {2,new Tuple<int,int>(10,11)}
        };
        

        [Command("startgame")]
        public async Task startGame() {
            await dealCards(); //Deal cards to all racers
            await shuffleRacers(); //Randomize Turn Order
            var guild = Context.Client.GetGuild (Context.Guild.Id);
            var user = guild.GetUser (Context.Client.CurrentUser.Id);
            await Context.Client.SetStatusAsync (UserStatus.Online);
            await Context.Client.SetGameAsync ("The 86th Trilliant Grand Prix", null, StreamType.NotStreaming);
            await displayCurrentBoard(true);
            await ReplyAsync("Game Started");
            await inGameAsync();
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

        [Command("discard")]
        public async Task discardAsync(int i) {
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
            await ReplyAsync(r.name + " discarded " + c.title);
            //Handle Survival Checks
            await SurvivalChecks(r);
            //IF Entire Turn Completed Successfully
            await endOfTurnLogic(r, i);
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
                    if(!r.canMove && racerID != 1) {
                        await ReplyAsync("You currently can't move. Try solving a hazard!");
                        return;
                    }
                    if(c.value > 2 && r.maxMove2 && racerID != 1) {
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
                                c = Card.get_card(12);
                                var h2 = r.hazards.Where(e=> e.item1.ID == remedy_to_hazards[(int)c.value].Item1 || e.item1.ID == remedy_to_hazards[(int)c.value].Item2 ).ToList();
                                if (h2 == null) {
                                    await ReplyAsync("You can't play this card. Try another.");
                                    return;
                                }
                                string s2 = h2[0].item1.title;
                                if(h2.Count > 1 ) {
                                    for(int j = 1; i < h2.Count; j++) {
                                        s2 += ", " + h2[j].item1.title;
                                    }
                                }
                                h2.ForEach(e=> {
                                    r.hazards.Remove(e);
                                    if(e.item1.ID == 5 || e.item1.ID == 8) {
                                        r.canMove = true;
                                    }
                                    if(e.item1.ID == 11) {
                                        r.maxMove2 = false;
                                    }
                                    if(e.item1.ID == 9) {
                                        r.sab = false;
                                    }
                                });
                                await ReplyAsync(r.name + " played " + c.title + " solving " + s2);
                                break;
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
                            var listRacer = racers.OrderBy(e=> e.distance).ToList();
                            listRacer.Reverse();
                            List<string> str = new List<string>();
                            str.Add(r.name + " causes debreis to hit " + listRacer[0].name);
                            listRacer[0].hazards.Add(new pair(c, 0));
                            for(int j = 1; j < 5 && j < listRacer.Count; j++) {
                                str.Add(listRacer[j].name);
                                listRacer[j].hazards.Add(new pair(c, 0));
                                listRacer[j].canMove = false;
                            }
                            string debreis = String.Join(", ",str);
                            await ReplyAsync(debreis);
                        break;
                        case 1:
                            string ram = r.name + " takes the ram action against ";
                            if(!r.canMove) {
                                await ReplyAsync("Sorry, you can't move. Try a different card.");
                                return;
                            } 
                            if(r.maxMove2) {
                                await ReplyAsync("Sorry, you can't move this far! Try a different card");
                                return;
                            }
                            r.distance += 3;
                            if (r.distance > 24) {
                                r.distance -= 3;
                                await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            racers.Where(e=>e.distance == r.distance+1).ToList().ForEach(e => {
                                e.canMove = false;
                                e.hazards.Add(new pair(c,0));
                                ram += e.name + " ";
                            });
                            await ReplyAsync(ram + " by moving 3 spaces!");
                        break;
                        case 2:
                            if(!r.canMove) {
                                await ReplyAsync("Sorry, you can't move. Try a different card.");
                                return;
                            }
                            r.distance++;
                            if (r.distance > 24) {
                                r.distance--;
                                await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            r.crash = true;
                            await ReplyAsync(r.name + " plays a **CRASH** card. You don't want to be on their space at the start of their next turn!");
                        break;
                    }
                    break;
                case "THazard":
                    var target = racers.FirstOrDefault(e=>e.ID == racerID);
                    if(target == null) {
                        await ReplyAsync("You didn't target a valid racer. Try again.");
                        return;
                    }
                    switch(c.value) {
                        case 0:
                            target.canMove = false;
                            target.hazards.Add(new pair(c,0));
                            await ReplyAsync(r.name + " played a " + c.title + " against " + target.name + ". They are unable to move until they solve this issue.");
                        break;
                        case 1:
                            target.sab = true;
                            target.hazards.Add(new pair(c,0));
                            await ReplyAsync(r.name + " played a " + c.title + " against " + target.name + ". They better not have any other hazards applied!");
                        break;
                        case 2:
                            target.hazards.Add(new pair(c,0));
                            await ReplyAsync(r.name + " played a " + c.title + " against " + target.name + ". They have 3 turns to solve this issue");
                        break;
                        case 3:
                            target.maxMove2 = true;
                            target.hazards.Add(new pair(c,0));
                            await ReplyAsync(r.name + " played a " + c.title + " against " + target.name + ". They are unable to move more than two spaces!");
                        break;
                    }
                    racer.replace_racer(target);
                    break;
                case "Remedy":
                    var h = r.hazards.Where(e=> e.item1.ID == remedy_to_hazards[(int)c.value].Item1 || e.item1.ID == remedy_to_hazards[(int)c.value].Item2 ).ToList();
                    if (h == null) {
                        await ReplyAsync("You can't play this card. Try another.");
                        return;
                    }
                    string solved = h[0].item1.title;
                    if(h.Count > 1 ) {
                        for(int j = 1; i < h.Count; j++) {
                            solved += ", " + h[j].item1.title;
                        }
                    }
                   h.ForEach(e=> {
                        r.hazards.Remove(e);
                        if(e.item1.ID == 5 || e.item1.ID == 8 || e.item1.ID == 6) {
                            r.canMove = true;
                        }
                        if(e.item1.ID == 11) {
                            r.maxMove2 = false;
                        }
                        if(e.item1.ID == 9) {
                            r.sab = false;
                        }
                    });
                    await ReplyAsync(r.name + " played " + c.title + " solving " + solved);
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
        [Command("ingame")]
        public async Task inGameAsync() {
            if (racers.Count == 0) {
                await ReplyAsync("No racers in game.");
                return;
            }
            List<string> str = new List<string>();
            str.Add("**Current Racers**");
            for(int i = 0; i < racers.Count; i++) {
                if(i == position) {
                    str.Add("**#" + i + ": " + racers[i].nameID() + "**");
                } else {
                    str.Add("#" + i + ": " + racers[i].nameID());
                }
            }
            string output = String.Join(System.Environment.NewLine, str);
            await ReplyAsync(output);
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
            round = 1;
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
                    await ReplyAsync(usr.Mention + ", you have won the race!");
                    await displayCurrentBoard();
                    await doReset();
                    return;
                }
                position -= racers.Count;
                round++;
            }
            await displayCurrentBoard();
            await nextTurn();
        }

        private async Task SurvivalChecks(racer r) { //Need to Finish Checking all Systems
            if(r.sab && r.hazards.Count > 1) {
                r.stillIn = false;
                await ReplyAsync(r.name + " subcumbs to Sabotage!");
            }
            foreach(pair p in r.hazards) {
                p.item2++;
                if(p.item2 > 2) {
                    r.stillIn = false;
                    await ReplyAsync(r.name + " subcumbs to " + p.item1.title + "!");
                }
            }
        }

        private async Task nextTurn() { //DM the next turn person
            var usr = Context.Guild.GetUser(racers[position].player_discord_id);
            if (usr == null ) {
                await ReplyAsync("Uhh boss, something went wrong.");
                return;
            }
            while(!racers[position].stillIn) {
                await ReplyAsync(racers[position].name + " is no longer in the race.");
                position++;
                if(position == racers.Count) {
                    await endOfTurn(); //Handle Passive Movement
                    racer winner = checkWinner();
                    if(winner != null) {
                        endGame = true;
                        var usr2 = Context.Guild.Users.FirstOrDefault(e=>e.Id == winner.player_discord_id);
                        await ReplyAsync(usr2 + ", you have won the race!");
                        return;
                    }
                    position -= racers.Count;
                    round++;
                    await displayCurrentBoard();
                }
                usr = Context.Guild.GetUser(racers[position].player_discord_id);
            }
            //Start of New Turn
            if(racers[position].crash) {
                List<string> str = new List<string>();
                str.Add(racers[position].name + "'s crash card triggers. The following racers crash out of the race.");
                racers.Where(e=>e.distance == racers[position].distance).ToList().ForEach(e=> {
                    if (e != racers[position]) {
                        e.stillIn = false;
                        str.Add(e.nameID());
                    }
                });
                string crashed = String.Join(System.Environment.NewLine,str);
                await ReplyAsync(crashed);
                racers[position].crash = false;
            }

            //DM Current Hand & Status
            List<string> str2 = new List<string>();
            str2.Add("**Your Current Hand**");
            if (racers[position].cards.Count == 0) { 
                await ReplyAsync("Hold up, you don't have any cards. The game must not have started yet.");
            } else {
                for(int i = 0; i < racers[position].cards.Count; i++) {
                    str2.Add("#" + (i+1) + ": " + racers[position].cards[i].ToString());
                }
                str2.Add("-- -- -- -- --");
                str2.Add("**Current Hazards** - If any Hazard is applied for 3 turns, you will explode.");
                if (racers[position].hazards.Count == 0) str2.Add("None");
                foreach (pair p in racers[position].hazards) {
                    str2.Add("-> " + p.item1.title +" has been applied for " + p.item2 + " turns. " + id_to_condition[p.item1.ID]);
                }
                string output = String.Join(System.Environment.NewLine, str2);
                await usr.SendMessageAsync(output);
            }
            await Context.Channel.SendMessageAsync(racers[position].name + " has the next turn.");
        }

        private Dictionary<int, string> id_to_condition = new Dictionary<int, string> {
            {5,"You cannot move until you play a Dodge card."},
            {6, "You cannot move until you play a Dodge card."},
            {8, "You cannot move until you play a Tech Savvy card."},
            {9, "Can be removed by a Tech Savvy card. If you end your turn with both Sabotage and another Hazard, you explode."},
            {10, "Can be removed by a Cyber Healthcare card."},
            {11, "You cannot play Movement cards higher than 2. Can be removed by a Cyber Healthcare card."}
        };

        private async Task displayCurrentBoard(bool first = false) {
            List<string> str = new List<string>();
            str.Add("**Leaderboard!** Turn " + round + "." + (position+1));
            str.Add("```");
            str.Add("Distance | Racer Name (ID) | Still In | Sponsor | Debris | Ramed | Crash | Mech Failure | Sabotage | Heart Attack | High G");
            var listRacer = racers.OrderBy(e=> e.distance).ToList();
            listRacer.Reverse();
            listRacer.ForEach(e=> str.Add(e.leader()));
            str.Add("```");
            string ouput_string = string.Join(System.Environment.NewLine, str);

            var channel = Context.Guild.GetTextChannel(Context.Guild.Channels.FirstOrDefault(e=>e.Name == "leaderboard").Id);
            if(!first) { 
                var messages = await channel.GetMessagesAsync(1).Flatten();
                await channel.DeleteMessagesAsync(messages);
            }
            await channel.SendMessageAsync(ouput_string);
            await ReplyAsync(ouput_string);
        }

        private async Task endOfTurn() {
            foreach (racer r in racers ) {
                r.distance++;
                if (r.distance > 24) {
                    r.distance = 24;
                }
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