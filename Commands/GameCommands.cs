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
using trillbot.Classes;

namespace trillbot.Commands
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        private static List<racer> racers = new List<racer>();
        private static Stack<Card> cards = new Stack<Card>();
        private static bool runningGame = false;
        private static int position = 0;
        private static int round = 1;
        private Dictionary<int, Tuple<int,int>> remedy_to_hazards = new Dictionary<int, Tuple<int,int>> {
            {0,new Tuple<int, int>(5,6)},
            {1,new Tuple<int,int>(8,9)},
            {2,new Tuple<int,int>(10,11)}
        };
        private Dictionary<int, string> target_hazard_output = new Dictionary<int, string> {
            {0, ". They are unable to move until they remove this Hazard."},
            {1, ". They better not have any other Hazards applied!"},
            {2,". They have 3 turns to remove this Hazard."},
            {3, ". They are unable to move more than two spaces!"}
        };

        [Command("initialize")]
        public async Task initAsync() {
            var game = new GrandPrix {
                channelName = Context.Channel.Name
            };
            Program.games.Add(Context.Channel.ID, game);
            await ReplyAsync("Game started and bound to this channel use `ta!joingame` in this channel to join.");
        }

        [Command("startgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task startGame() {
            await dealCards(); //Deal cards to all racers
            await shuffleRacers(); //Randomize Turn Order
            var guild = Context.Client.GetGuild (Context.Guild.Id);
            var user = guild.GetUser (Context.Client.CurrentUser.Id);
            runningGame = true;
            await Context.Client.SetStatusAsync (UserStatus.Online);
            await Context.Client.SetGameAsync ("The 86th Trilliant Grand Prix", null, StreamType.NotStreaming);
            await displayCurrentBoard();
            //await ReplyAsync("Game Started");
            await inGameAsync();
            await nextTurn();
        }

        [Command("joingame")]
        public async Task joinGame() {
            racer racer = racer.get_racer(Context.Message.Author.Id);
            if (runningGame) {
                await ReplyAsync("The game has already started");
                return;
            }

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
                var usr = Context.Guild.GetUser(r.player_discord_id);
                await usr.SendMessageAsync(r.currentStatus());
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
        public async Task playCardAsync(int i, int racerID = -1, int hazardID = -1) {
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
                    if(!r.canMove() && racerID != 1) {
                        await ReplyAsync("You currently can't move. Try solving a hazard!");
                        return;
                    }
                    if(c.value > 2 && r.maxMove2() && racerID != 1) {
                        await ReplyAsync("You currently can't move more than 2 spaces. Try solving a hazard!");
                        return;
                    }
                    switch(c.value) {
                        case 6:
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
                            var listRacer = racers.OrderByDescending(e=> e.distance).ToList();
                            List<string> str = new List<string>();
                            str.Add(r.name + " causes debris to hit " + listRacer[0].name);
                            listRacer[0].addHazard(c);
                            for(int j = 1; j < 5 && j < listRacer.Count; j++) {
                                str.Add(listRacer[j].name);
                                listRacer[j].addHazard(c);
                            }
                            string debreis = String.Join(", ",str);
                            await ReplyAsync(debreis);
                        break;
                        case 1:
                            string ram = "";
                            if(!r.canMove()) {
                                await ReplyAsync("Sorry, you can't move. Try a different card.");
                                return;
                            } 
                            if(r.maxMove2()) {
                                await ReplyAsync("Sorry, you can't move this far! Try a different card");
                                return;
                            }
                            var targets = racers.Where(e=>e.distance == r.distance+1).ToList();
                            r.distance += 3;
                            if (r.distance > 24) {
                                r.distance -= 3;
                                await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            if(targets.Count == 0) {
                                ram = r.name + " plays RAM and doesn't hit any other racers! They move forward 3 spaces to a distance of " + r.distance;
                            } else {
                                List<string> tar = new List<string>();
                                tar.Add(r.name + " plays " + c.title + " against " + tar[0]);
                                for(int j = 1; j < targets.Count; j++ ) {
                                    targets[j].addHazard(c);
                                    tar.Add(targets[j].name);
                                };
                                ram = String.Join(", ",tar);
                                ram += ". " + r.name + " moves forward 3 spaces to a distance of " + r.distance;
                            }
                            await ReplyAsync(ram);
                        break;
                        case 2:
                            if(!r.canMove()) {
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
                        case 3:
                            if(!r.canMove()) {
                                await ReplyAsync("You currently can't move. Try solving a hazard!");
                                return;
                            }
                            var listRacer2 = racers.OrderByDescending(e=> e.distance).ToList();
                            List<string> str3 = new List<string>();
                            int z = 3;
                            bool foundR = false;
                            for(int j = 0; j < listRacer2.Count && j < z; j++) {
                                if(listRacer2[j] == r) {
                                    foundR = true;
                                    z++;
                                    continue;
                                } else if(!foundR) {
                                    z++;
                                }
                                if(foundR) {
                                    if (z-j == 3) {
                                        str3.Add(r.name + " dazzles  " + listRacer2[j].name);
                                        listRacer2[j].addHazard(c);
                                    } else {
                                        str3.Add(listRacer2[j].name);
                                        listRacer2[j].addHazard(c);
                                    }
                                }
                            }
                            r.distance++;
                            string debris = String.Join(", ",str3);
                            await ReplyAsync(debris + ". " + r.name + " also moves one space.");
                        break;
                    }
                    break;
                case "THazard":
                    var target = racers.FirstOrDefault(e=>e.ID == racerID);
                    if(target == null) {
                        await ReplyAsync("You didn't target a valid racer. Try again.");
                        return;
                    }
                    if (target == r) {
                        await ReplyAsync("You can't target yourself...");
                        return;
                    }
                    await ReplyAsync(targetHazard(r,target,c));
                    racer.replace_racer(target);
                    break;
                case "Remedy":
                    switch(c.value) {
                        case 3:
                        if(racerID < 0 || hazardID < 0) {
                            await ReplyAsync("You need to provide two hazards to target. If you only have one provide a `0` as the other input.");
                            return;
                        }
                        if(--racerID > r.hazards.Count || --hazardID > r.hazards.Count) {
                            await ReplyAsync("One of your inputs is outside of acceptable limits. Please try again");
                            return;
                        }
                        if(racerID == -1) {
                            await ReplyAsync(r.name + " played " + c.title.ToLower() + " solving " + r.hazards[hazardID].item1.title.ToLower());
                            r.hazards.RemoveAt(hazardID);
                        } else if (hazardID == -1) {
                            await ReplyAsync(r.name + " played " + c.title.ToLower() + " solving " + r.hazards[racerID].item1.title.ToLower());
                            r.hazards.RemoveAt(racerID);
                        } else {
                            await ReplyAsync(r.name + " played " + c.title.ToLower() + " solving " + r.hazards[racerID].item1.title.ToLower() + " and " + r.hazards[hazardID].item1.title.ToLower());
                            if(racerID > hazardID) {   
                                r.hazards.RemoveAt(racerID);
                                r.hazards.RemoveAt(hazardID);
                            } else {
                                r.hazards.RemoveAt(hazardID);
                                r.hazards.RemoveAt(racerID);
                            }
                        }
                        r.distance-=2;
                        break;
                        default:
                        var h = r.hazards.Where(e=> e.item1.ID == remedy_to_hazards[(int)c.value].Item1 || e.item1.ID == remedy_to_hazards[(int)c.value].Item2 ).ToList();
                        if (h.Count == 0) {
                            await ReplyAsync("You can't play this card. Try another.");
                            return;
                        }
                        List<string> str = new List<string>();
                        h.ForEach(e=> {
                            r.hazards.Remove(e);
                            str.Add(e.item1.title.ToLower());
                        });
                        string solved = String.Join(", ",str);
                        await ReplyAsync(r.name + " played " + c.title + " solving " + solved);
                        break;
                    }
                    break;
                default:
                    await ReplyAsync("Um boss, something went wrong. Let's try again!");
                    return;
            }
            //Handle Survival Checks
            await SurvivalChecks(r);
            //IF Entire Turn Completed Successfully
            if(oneAlive()) {
                await endOfTurnLogic(r, i);
            } else {
                await ReplyAsync("All racers are dead. This ends the game.");
                await doReset();
                return;
            }
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
                    str.Add("**#" + (i+1) + ": " + racers[i].nameID() + "**");
                } else {
                    str.Add("#" + (i+1) + ": " + racers[i].nameID());
                }
            }
            string output = String.Join(System.Environment.NewLine, str);
            await ReplyAsync(output);
        }

        [Command("reset")] //Reset the game to initial state
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task doReset() {
            cards = new Stack<Card>();
            racers.ForEach(e=> {
                e.reset();
                racer.replace_racer(e);
            });
            racers = new List<racer>();
            position = 0;
            round = 1;
            runningGame = false;
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

        private string targetHazard(racer racer, racer target, Card card) {
            target.addHazard(card);
            return (racer.name + " played a " + card.title + " against " + target.name + target_hazard_output[(int)card.value]);
        }

        private async Task endOfTurnLogic(racer r, int i) { //Handle All Logic for Transitioning an End of Turn
            r.cards.RemoveAt(i);
            if(cards.Count == 0 ) cards = generateDeck();
            r.cards.Add(cards.Pop());
            racer.update_racer(r);
            position++;
            if(position == racers.Count) {
                await endOfTurn(); //Handle Passive Movement
                position -= racers.Count;
                round++;
            }
            racer winner = checkWinner();
                if(winner != null) {
                    SocketGuildUser usr = Context.Guild.Users.FirstOrDefault(e=>e.Id == winner.player_discord_id);
                    await ReplyAsync(usr.Mention + ", you have won the race!");
                    await displayCurrentBoard();
                    await doReset();
                    return;
                }
            await displayCurrentBoard();
            await nextTurn();
        }

        private async Task SurvivalChecks(racer r) { 
            if(r.sab() && r.hazards.Count > 1) {
                r.stillIn = false;
                await ReplyAsync(r.name + " subcumbs to Sabotage and their vehicle explodes!");
            }
            pair remove = null;
            r.hazards.ForEach(e=>{
                e.item2++;
                if(e.item2 > 2)
                {
                    r.stillIn = false;
                    ReplyAsync(r.name + " subcumbs to " + e.item1.title + " and their vehicle explodes!");
                }

                if(e.item1.ID == 16 && e.item2 > 0) {
                    remove = e;
                }
            });
            if(remove != null) {
                r.hazards.Remove(remove);
            }
            if (r.distance < 0) {
                r.distance = 0;
            }
        }

        private async Task nextTurn() { //DM the next turn person
            var usr = Context.Guild.GetUser(racers[position].player_discord_id);
            if (usr == null ) {
                await ReplyAsync("Uhh boss, something went wrong.");
                return;
            }
            List<string> outOfRace = new List<string>();
            while(!racers[position].stillIn) {
                outOfRace.Add(racers[position].name + " is no longer in the race.");
                position++;
                if(position == racers.Count) {
                    await endOfTurn(); //Handle Passive Movement
                    racer winner = checkWinner();
                    if(winner != null) {
                        var usr2 = Context.Guild.Users.FirstOrDefault(e=>e.Id == winner.player_discord_id);
                        await ReplyAsync(usr2.Mention + ", you have won the race!");
                        await doReset();
                        return;
                    }
                    position -= racers.Count;
                    round++;
                    await displayCurrentBoard();
                }
                usr = Context.Guild.GetUser(racers[position].player_discord_id);
            }
            if(outOfRace.Count != 0) {
                string output_outOfRace = String.Join(System.Environment.NewLine,outOfRace);
                await ReplyAsync(output_outOfRace);
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
            await usr.SendMessageAsync(racers[position].currentStatus());

            //Start Next Turn
            await Context.Channel.SendMessageAsync(racers[position].name + " has the next turn.");
        }

        private async Task displayCurrentBoard() {
            List<string> str = new List<string>();
            str.Add("**Leaderboard!** Turn " + round + "." + (position+1));
            str.Add("```");
            str.Add("Distance | Racer Name (ID) | Still In | Sponsor | Special Ability | Hazards ");
            var listRacer = racers.OrderByDescending(e=> e.distance).ToList();
            listRacer.ForEach(e=> str.Add(e.leader()));
            str.Add("```");
            string ouput_string = string.Join(System.Environment.NewLine, str);
            await ReplyAsync(ouput_string);
        }

        private bool oneAlive() {
            bool alive = false;
            foreach(racer r in racers) {
                if(r.stillIn) { 
                    alive = true;
                    break;
                }
            }
            return alive;
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
                if (r.distance == 24 && r.stillIn) {
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