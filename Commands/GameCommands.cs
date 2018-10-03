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

        [Command("startgame")]
        public async Task startGame() {
            shuffleRacers();
            await ReplyAsync("Game Started");
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

        [Command("dealDeck")]
        public async Task dealCards()
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
            if (1 > i && i > 10) {
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
                                break;
                            } else if (racerID == 0) {
                                r.distance += c.value;
                                if (r.distance > 24) {
                                    r.distance -= c.value;
                                    await ReplyAsync("Woah there, you can't move past space 24! Try a different card.");
                                    return;
                                }
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
            if(r.sab && r.hazards.Count > 1) {
                r.stillIn = false;
                await ReplyAsync(r.name + " subcumbs to Sabotage!");
            }
            foreach(Tuple<int,int> t in r.hazards) {
                t.Item2++;
                if(t.Item2 > 2) {
                    r.stillIn = false;
                    await ReplyAsync(r.name + " subcumbs to " + Card.get_card().FirstOrDefault(e=>e.ID == t.Item1).title + "!";
                }
            }
            //IF Entire Turn Completed Successfully
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
                }
                position -= racers.Count;
            }
            await ReplyAsync("");
        }

        [Command("reset")]
        public async Task doReset() {
            cards = new Stack<Card>();
            foreach (racer r in racers) {
                r.resetCards();
                racer.update_racer(r);
            }
            racers = new List<racer>();
            position = 0;
            await ReplyAsync("Game Reset");
        }

        private async Task endOfTurn() {
            racers.ForEach(e=>e.distance++);
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

        private void shuffleRacers() {
            List<racer> temp = new List<racer>();

            while (racers.Count > 0) {
                int num = trillbot.Program.rand.Next(0,racers.Count);
                temp.Add(racers[num]);
                racers.RemoveAt(num);
            }

            racers = temp;
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