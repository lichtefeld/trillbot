using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;
using trillbot.Classes;

namespace trillbot.Classes {
    public class GrandPrix {
        private List<racer> racers = new List<racer>();
        private Stack<Card> cards = new Stack<Card>();
        private bool runningGame = false;
        private int position = 0;
        private int round = 1;
        public string channelName { get; set; } = "";
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

        //Private Helper Functions
        //Output Function
        private void output(ISocketMessageChannel channel, List<string> str) {
            int count = 0;
            string output_string = "";
            foreach(string s in str) {
                count += s.Length + 1;
                if (count >= 2000) {
                    channel.SendMessageAsync(output_string);
                    count = s.Length;
                    output_string = s + System.Environment.NewLine;
                } else {
                    output_string += s + System.Environment.NewLine;
                }
            }
            channel.SendMessageAsync(output_string);
        }

        private async Task output(ISocketMessageChannel channel, string str) {
            if (str.Length > 2000) {
                //Handle Being Passed a String longer than 2k characers
            } else {
                await channel.SendMessageAsync(str);
            }
        }

        //Deal Cards
        private  void dealCards(SocketCommandContext Context)
        {
            cards = generateDeck();
            foreach(racer r in racers) {
                for(int i = 0; i < 8; i++) {
                    if(cards.Count == 0) { cards = generateDeck(); }
                    r.cards.Add(cards.Pop());
                }
                racer.update_racer(r);
                var usr = Context.Guild.GetUser(r.player_discord_id);
                usr.SendMessageAsync(r.currentStatus());
            }
        }

        //Hazard Output
        private string targetHazard(racer racer, racer target, Card card) {
            target.addHazard(card);
            return (racer.name + " played a " + card.title + " against " + target.name + target_hazard_output[(int)card.value]);
        }

        //End of Turn Logic
        private void endOfTurnLogic(SocketCommandContext Context, racer r, int i) { //Handle All Logic for Transitioning an End of Turn
            r.cards.RemoveAt(i);
            if(cards.Count == 0 ) cards = generateDeck();
            r.cards.Add(cards.Pop());
            racer.update_racer(r);
            position++;
            if(position == racers.Count) {
                endOfTurn(Context); //Handle Passive Movement
                position -= racers.Count;
                round++;
            }
            racer winner = checkWinner();
                if(winner != null) {
                    SocketGuildUser usr = Context.Guild.Users.FirstOrDefault(e=>e.Id == winner.player_discord_id);
                    output(Context.Channel,usr.Mention + ", you have won the race!");
                    displayCurrentBoard(Context);
                    doReset(Context);
                    return;
                }
            displayCurrentBoard(Context);
            nextTurn(Context);
        }

        //Survival Checks
        private static List<string> SurvivalChecks(SocketCommandContext Context, racer r) { 
            var str = new List<string>();
            if(r.sab() && r.hazards.Count > 1) {
                r.stillIn = false;
                str.Add(r.name + " subcumbs to Sabotage and their vehicle explodes!");
            }
            pair remove = null;
            r.hazards.ForEach(e=>{
                e.item2++;
                if(e.item2 > 2)
                {
                    r.stillIn = false;
                    str.Add(r.name + " subcumbs to " + e.item1.title + " and their vehicle explodes!");
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
            return str;
        }

        //Increment Turn
        private void nextTurn(SocketCommandContext Context) { //DM the next turn person
            var usr = Context.Guild.GetUser(racers[position].player_discord_id);
            if (usr == null ) {
                Context.Channel.SendMessageAsync("Uhh boss, something went wrong.");
                return;
            }
            List<string> outOfRace = new List<string>();
            while(!racers[position].stillIn) {
                outOfRace.Add(racers[position].name + " is no longer in the race.");
                position++;
                if(position == racers.Count) {
                    endOfTurn(Context); //Handle Passive Movement
                    racer winner = checkWinner();
                    if(winner != null) {
                        var usr2 = Context.Guild.Users.FirstOrDefault(e=>e.Id == winner.player_discord_id);
                        Context.Channel.SendMessageAsync(usr2.Mention + ", you have won the race!");
                        doReset(Context);
                        return;
                    }
                    position -= racers.Count;
                    round++;
                    displayCurrentBoard(Context);
                }
                usr = Context.Guild.GetUser(racers[position].player_discord_id);
            }
            if(outOfRace.Count != 0) {
                string output_outOfRace = String.Join(System.Environment.NewLine,outOfRace);
                Context.Channel.SendMessageAsync(output_outOfRace);
            }

            //Start of New Turn
            if(racers[position].crash) {
                List<string> str = new List<string>();
                str.Add(racers[position].name + "'s crash card triggers.");
                racers.Where(e=>e.distance == racers[position].distance).ToList().ForEach(e=> {
                    if (e != racers[position]) {
                        e.stillIn = false;
                        str.Add(e.nameID());
                    }
                });
                string crashed = String.Join(System.Environment.NewLine,str);
                Context.Channel.SendMessageAsync(crashed);
                racers[position].crash = false;
            }

            //DM Current Hand & Status
            if (!(position == 0 && round == 1)) usr.SendMessageAsync(racers[position].currentStatus());

            //Start Next Turn
            Context.Channel.SendMessageAsync(racers[position].name + " has the next turn.");
        }

        //Leaderboard Output
        private void displayCurrentBoard(SocketCommandContext Context) {
            List<string> str = new List<string>();
            str.Add("**Leaderboard!** Turn " + round + "." + (position+1));
            str.Add("```");
            str.Add("Distance | Racer Name (ID) | Still In | Sponsor | Special Ability | Hazards ");
            var listRacer = racers.OrderByDescending(e=> e.distance).ToList();
            listRacer.ForEach(e=> str.Add(e.leader()));
            str.Add("```");
            string ouput_string = string.Join(System.Environment.NewLine, str);
            Context.Channel.SendMessageAsync(ouput_string);
        }

        //One Player Still Alive?
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

        //Passive Movement
        private void endOfTurn(SocketCommandContext Context) {
            foreach (racer r in racers ) {
                r.distance++;
                if (r.distance > 24) {
                    r.distance = 24;
                }
                racer.update_racer(r);
            }
        }

        //Check for Winner [Return Racer]
        private racer checkWinner() {
            foreach(racer r in racers) {
                if (r.distance == 24 && r.stillIn) {
                    return r;
                }
            }
            return null;
        }

        //Shuffle Racers
        private void shuffleRacers(SocketCommandContext Context) {
            List<racer> temp = new List<racer>();

            while (racers.Count > 0) {
                int num = trillbot.Program.rand.Next(0,racers.Count);
                temp.Add(racers[num]);
                racers.RemoveAt(num);
            }

            racers = temp;
        }

        //Make New Deck of Shuffled Cards
        private static Stack<Card> generateDeck() {
            return shuffleDeck(freshDeck());
        }

        //Make a fresh deck of cards
        private static List<Card> freshDeck() {
            List<Card> c = new List<Card>();
            List<Card> temp = trillbot.Classes.Card.get_card();

            foreach (Card c1 in temp) {
                for(int i = 0; i < c1.count; i++) {
                    c.Add(c1);
                }
            }

            return c;
        }

        //Shuffle a deck of cards
        private static Stack<Card> shuffleDeck(List<Card> c) {
            Stack<Card> s = new Stack<Card>();

            while (c.Count > 0) {
                int num = trillbot.Program.rand.Next(0,c.Count);
                s.Push(c[num]);
                c.RemoveAt(num);
            }

            return s;
        }

        //Start Game
        public void startGame(SocketCommandContext Context) {
            dealCards(Context); //Deal cards to all racers
            shuffleRacers(Context); //Randomize Turn Order
            runningGame = true;
            Context.Client.SetStatusAsync (UserStatus.Online);
            Context.Client.SetGameAsync ("The 86th Trilliant Grand Prix", null, StreamType.NotStreaming);
            displayCurrentBoard(Context);
            inGameAsync(Context);
            nextTurn(Context);
        }

        //Join Game
        public void joinGame(SocketCommandContext Context) {
            racer racer = racer.get_racer(Context.Message.Author.Id);
            if (runningGame) {
                Context.Channel.SendMessageAsync("The game has already started");
                return;
            }

            if(racer == null) {
                Context.Channel.SendMessageAsync("No racer found for you, " + Context.User.Mention);
                return;
            } else {
                if(racer.inGame) {
                    Context.Channel.SendMessageAsync("Hold up," +  Context.User.Mention + ", youcan only play in one game at a time!");
                    return;
                }
                foreach (racer r in racers) {
                    if(r.ID == racer.ID) {
                        Context.Channel.SendMessageAsync(Context.User.Mention + ", you have already joined the game!");
                        return;
                    }
                }
                racer.inGame = true;
                racer.replace_racer(racer);
                racers.Add(racer);
            }

            Context.Channel.SendMessageAsync(Context.User.Mention + ", you have joined the game");
        }

        //Discard a Card
        public void discardAsync(SocketCommandContext Context, int i) {
            racer r = racers[position];
            if(r.player_discord_id != Context.Message.Author.Id) {
                Context.Channel.SendMessageAsync("It's not your turn!");
                return;
            }
            if (i < 1 && i > 8) {
                Context.Channel.SendMessageAsync("You only have 8 cards in your hand! Provide a number 1-8.");
                return;
            }
            Card c = r.cards[--i];
            Context.Channel.SendMessageAsync(r.name + " discarded " + c.title);
            //Handle Survival Checks
            SurvivalChecks(Context, r);
            //IF Entire Turn Completed Successfully
            endOfTurnLogic(Context, r, i);
        }

        //Play a Card
        public void playCardAsync(SocketCommandContext Context, int i, int racerID = -1, int hazardID = -1) {
            racer r = racers[position];
            if(r.player_discord_id != Context.Message.Author.Id) {
                Context.Channel.SendMessageAsync("It's not your turn!");
                return;
            }
            if (i < 1 && i > 8) {
                Context.Channel.SendMessageAsync("You only have 8 cards in your hand! Provide a number 1-8.");
                return;
            }
            Card c = r.cards[--i];
            switch(c.type) { 
                case "Movement":
                    if(!r.canMove() && racerID != 1) {
                        Context.Channel.SendMessageAsync("You currently can't move. Try solving a hazard!");
                        return;
                    }
                    if(c.value > 2 && r.maxMove2() && racerID != 1) {
                        Context.Channel.SendMessageAsync("You currently can't move more than 2 spaces. Try solving a hazard!");
                        return;
                    }
                    switch(c.value) {
                        case 6:
                            if (racerID <= -1 || racerID > 1) {
                                Context.Channel.SendMessageAsync("You didn't indicate which version of this card to use.");
                                return;
                            }
                            else if (racerID == 1) {
                                c = Card.get_card(12);
                                var h2 = r.hazards.Where(e=> e.item1.ID == remedy_to_hazards[(int)c.value].Item1 || e.item1.ID == remedy_to_hazards[(int)c.value].Item2 ).ToList();
                                if (h2 == null) {
                                    Context.Channel.SendMessageAsync("You can't play this card. Try another.");
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
                                Context.Channel.SendMessageAsync(r.name + " played " + c.title + " solving " + s2);
                                break;
                            } else if (racerID == 0) {
                                r.distance += c.value;
                                if (r.distance > 24) {
                                    r.distance -= c.value;
                                    Context.Channel.SendMessageAsync("Woah there, you can't move past space 24! Try a different card.");
                                    return;
                                }
                                Context.Channel.SendMessageAsync(r.name + " played a " + c.title + " spaces. They are now at a distance of " + r.distance + " units." );
                            }
                            break;
                        case 1:
                        case 2:
                        case 3:
                        case 4:
                            r.distance += c.value;
                            if (r.distance > 24) {
                                r.distance -= c.value;
                                Context.Channel.SendMessageAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            Context.Channel.SendMessageAsync(r.name + " played a " + c.title + " spaces. They are now at a distance of " + r.distance + " units." );
                            break;
                        default:
                            Context.Channel.SendMessageAsync("Um boss, something went wrong. Let's try again!");
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
                            Context.Channel.SendMessageAsync(debreis);
                        break;
                        case 1:
                            string ram = "";
                            if(!r.canMove()) {
                                Context.Channel.SendMessageAsync("Sorry, you can't move. Try a different card.");
                                return;
                            } 
                            if(r.maxMove2()) {
                                Context.Channel.SendMessageAsync("Sorry, you can't move this far! Try a different card");
                                return;
                            }
                            var targets = racers.Where(e=>e.distance == r.distance+1).ToList();
                            r.distance += 3;
                            if (r.distance > 24) {
                                r.distance -= 3;
                                Context.Channel.SendMessageAsync("Woah there, you can't move past space 24! Try a different card.");
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
                            Context.Channel.SendMessageAsync(ram);
                        break;
                        case 2:
                            if(!r.canMove()) {
                                Context.Channel.SendMessageAsync("Sorry, you can't move. Try a different card.");
                                return;
                            }
                            r.distance++;
                            if (r.distance > 24) {
                                r.distance--;
                                Context.Channel.SendMessageAsync("Woah there, you can't move past space 24! Try a different card.");
                                return;
                            }
                            r.crash = true;
                            Context.Channel.SendMessageAsync(r.name + " plays a **CRASH** card. You don't want to be on their space at the start of their next turn!");
                        break;
                        case 3:
                            if(!r.canMove()) {
                                Context.Channel.SendMessageAsync("You currently can't move. Try solving a hazard!");
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
                            Context.Channel.SendMessageAsync(debris + ". " + r.name + " also moves one space.");
                        break;
                    }
                    break;
                case "THazard":
                    var target = racers.FirstOrDefault(e=>e.ID == racerID);
                    if(target == null) {
                        Context.Channel.SendMessageAsync("You didn't target a valid racer. Try again.");
                        return;
                    }
                    if (target == r) {
                        Context.Channel.SendMessageAsync("You can't target yourself...");
                        return;
                    }
                    Context.Channel.SendMessageAsync(targetHazard(r,target,c));
                    racer.replace_racer(target);
                    break;
                case "Remedy":
                    switch(c.value) {
                        case 3:
                        if(racerID < 0 || hazardID < 0) {
                            Context.Channel.SendMessageAsync("You need to provide two hazards to target. If you only have one provide a `0` as the other input.");
                            return;
                        }
                        if(--racerID > r.hazards.Count || --hazardID > r.hazards.Count) {
                            Context.Channel.SendMessageAsync("One of your inputs is outside of acceptable limits. Please try again");
                            return;
                        }
                        if(racerID == -1) {
                            Context.Channel.SendMessageAsync(r.name + " played " + c.title.ToLower() + " solving " + r.hazards[hazardID].item1.title.ToLower());
                            r.hazards.RemoveAt(hazardID);
                        } else if (hazardID == -1) {
                            Context.Channel.SendMessageAsync(r.name + " played " + c.title.ToLower() + " solving " + r.hazards[racerID].item1.title.ToLower());
                            r.hazards.RemoveAt(racerID);
                        } else {
                            Context.Channel.SendMessageAsync(r.name + " played " + c.title.ToLower() + " solving " + r.hazards[racerID].item1.title.ToLower() + " and " + r.hazards[hazardID].item1.title.ToLower());
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
                            Context.Channel.SendMessageAsync("You can't play this card. Try another.");
                            return;
                        }
                        List<string> str = new List<string>();
                        h.ForEach(e=> {
                            r.hazards.Remove(e);
                            str.Add(e.item1.title.ToLower());
                        });
                        string solved = String.Join(", ",str);
                        Context.Channel.SendMessageAsync(r.name + " played " + c.title + " solving " + solved);
                        break;
                    }
                    break;
                default:
                    Context.Channel.SendMessageAsync("Um boss, something went wrong. Let's try again!");
                    return;
            }
            //Handle Survival Checks
            SurvivalChecks(Context, r);
            //IF Entire Turn Completed Successfully
            if(oneAlive()) {
                endOfTurnLogic(Context, r, i);
            } else {
                Context.Channel.SendMessageAsync("All racers are dead. This ends the game.");
                doReset(Context);
                return;
            }
        }
        
        //Display Current players in the game
        public void inGameAsync(SocketCommandContext Context) {
            if (racers.Count == 0) {
                Context.Channel.SendMessageAsync("No racers in game.");
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
            Context.Channel.SendMessageAsync(output);
        }

        //Mannually Reset a Game
        public void doReset(SocketCommandContext Context) {
            racers.ForEach(e=> {
                e.reset();
                racer.replace_racer(e);
            });
            Context.Channel.SendMessageAsync("Game Reset");
        }

        //Ping the current players turn
        public void whosTurnAsync(SocketCommandContext Context) { //Need to remind a person its there turn?
            var usr = Context.Guild.GetUser(racers[position].player_discord_id);
            if (usr == null ) {
                Context.Channel.SendMessageAsync("Uhh boss, something went wrong.");
            }
            Context.Channel.SendMessageAsync( "Hey " + usr.Mention + " It's your turn now!");
        }

    }

}