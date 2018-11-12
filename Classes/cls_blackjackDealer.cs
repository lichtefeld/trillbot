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

    public class blackjackDealer {
        private Stack<StandardCard> deck { get; set; } = new Stack<StandardCard>();
        private int numberOfDecks { get; set; }
        private int CardsUntilShuffle { get; set; }
        public string dealerName { get; set; }
        public List<blackjackPlayer> table {get; set;} = new List<blackjackPlayer>();
        private Tuple<int,int> position { get; set; } = new Tuple<int,int>(0,0);
        private int currentRound {get; set; }
        private List<ulong> toLeave { get; set; } = new List<ulong>();
        private bool gameRunning { get; set; } = false;
        private ISocketMessageChannel channel { get; set;}
        private List<StandardCard> hand { get; set; } = new List<StandardCard>();
        public int minbet { get; set; }
        public int maxbet { get; set; }
        private List<string> toOutput { get; set; } = new List<string>();

        //List of Players to Add, List of Players to Remove

        public blackjackDealer(string name, int numDecks, ISocketMessageChannel c, int minBet, int maxBet) {
            dealerName = name;
            numberOfDecks = numDecks;
            channel = c;
            CardsUntilShuffle = -1;
            minbet = minBet;
            maxbet = maxBet;
            //newDeck();
        }  

        private void newDeck() {
            List<StandardCard> cards = StandardCard.straightDeck();
            List<StandardCard> multiCard = cards;
            for(int i = 1; i < numberOfDecks; i++) {
                multiCard.AddRange(cards);
            }
            CardsUntilShuffle = multiCard.Count/2 + Program.rand.Next(52*(numberOfDecks/2));
            this.deck = StandardCard.shuffleDeck(cards);
        }

        public void addPlayer(blackjackPlayer p, SocketCommandContext Context) {
            table.Add(p);
            if (!gameRunning) {
                helpers.output(channel,Context.User.Mention + ", you have sat down at " + this.dealerName + "'s table. You'll be betting " + p.bet + " on every hand.");
                if(table.Count == 1) runGame();
                else helpers.output(channel,"I will wait for `ta!next` to start the next round");
            } else {
                helpers.output(channel,Context.User.Mention + ", you have sat down at " + this.dealerName + "'s table and will join the next game. You'll be betting " + p.bet + " on every hand.");
            }
        }

        public void subPlayer(SocketCommandContext Context) {
            var player = table.FirstOrDefault(e=> e.player_discord_id == Context.User.Id);
            if(player != null) {
                if(gameRunning) {
                    toLeave.Add(Context.User.Id);
                    helpers.output(channel,Context.User.Mention + ", you nod and will leave this table at the end of the round.");
                } else {
                    table.Remove(player);
                    helpers.output(channel,Context.User.Mention + ", you stand up and leave the table." + System.Environment.NewLine + dealerName + " sits and wait for someone to approach his table.");
                    CardsUntilShuffle = -1;
                }
            }
        }

        private void subPlayer() {
            foreach(var i in toLeave) {
                table.Remove(table.FirstOrDefault(e=> e.player_discord_id == i));
            }
        }

        private void collectBets() {
            foreach(blackjackPlayer p in table) {
                var c = Character.get_character(p.player_discord_id);
                if (c != null) {
                    if(c.balance - p.bet >= 0) {
                        c.balance -= p.bet;
                        Character.update_character(c);
                    } else {
                        toOutput.Add(p.name + " is unable to make this bet and they leave the table.");
                        toLeave.Add(p.player_discord_id);
                    }
                }
            }
        }

        private StandardCard GetCard() {
            CardsUntilShuffle--;
            if (deck.Count == 0) newDeck();
            return deck.Pop();
        }

        private void dealHand() {
            if(CardsUntilShuffle < 0) newDeck();
            foreach(blackjackPlayer p in table) {
                p.hand[0].Add(GetCard());
                p.hand[0].Add(GetCard());
                p.insurance = -1;
            }
            hand.Add(GetCard());
            hand.Add(GetCard());
        }

        private void displayTable(bool firstRound) {
            //List<string> str = new List<string>();
            foreach(blackjackPlayer p in table) {
                var s = p.handDisplay();
                toOutput.Add(s);
            }
            toOutput.Add(handDisplay(hand,!firstRound));
        }

        private string handDisplay(List<StandardCard> cards, bool holeCard) { //Dealer Version 
            List<string> str = new List<string>();
            if(holeCard) {
                foreach(var c in cards) {
                    str.Add(c.ToString());
                }
                str.Add(handValue(cards).ToString());
            } else {
                str.Add("XX");
                for(int i = 1; i < cards.Count; i++) {
                    str.Add(cards[i].ToString());
                }
            }
            return "**" + dealerName + "'s Hand**" + System.Environment.NewLine + String.Join(" | ", str);
        }

        private void dealerTurn() {
            //var str = new List<string>();
            toOutput.Add(handDisplay(hand,true));
            while(handValue(hand) < 17) {
                hand.Add(GetCard());
                toOutput.Add(handDisplay(hand,true));
            }
            payouts();
        }

        private void nextPlayer() {
            if (position.Item1 >= currentRound) {
                dealerTurn();
            } else {
                var usr = channel.GetUserAsync(table[position.Item1].player_discord_id).GetAwaiter().GetResult();
                if(table[position.Item1].handValue(position.Item2) == 21) {
                    toOutput.Add(usr.Mention + ", it is now your turn. " + System.Environment.NewLine + table[position.Item1].handDisplay() + System.Environment.NewLine + table[position.Item1].name + ", you have 21. Automatically standing");
                    position = new Tuple<int, int>(position.Item1+1,0);
                    nextPlayer();
                    return;
                }
                toOutput.Add(usr.Mention + ", it is now your turn. " + System.Environment.NewLine + table[position.Item1].handDisplay() + System.Environment.NewLine + table[position.Item1].name + ", would you like to hit, stand, double, split, or surrender?");
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
            }
        }

        private int handValue(List<StandardCard> cards) {
            int value = 0;
            int numAces = 0;
            foreach(StandardCard c in cards) {
                if(c.value == 14) {
                    numAces++;
                    value += 11;
                } else if(c.value > 10) {
                    value += 10;
                } else {
                    value += c.value;
                }
            }
            while(numAces > 0 && value > 21) {
                value -= 10;
                numAces--;
            }
            return value;
        }

        private void checkDealer() {
            if(hand[1].value == 14) {
                toOutput.Add(dealerName + " offers insurance to the table.");
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
                return;
            } else if (handValue(hand) == 21) {
                toOutput.Add(dealerName + " has blackjack! Hand: " + hand[0].ToString() + " | " + hand[1].ToString());
                payouts();
            } else {
                nextPlayer();
            }
        }

        private void payouts() {
            position = new Tuple<int,int>(0,0);
            //var str = new List<string>();
            //Check if Dealer Has Blackjack?
            if(handValue(hand) == 21 && hand.Count == 2) { //Dealer Has Blackjack!
                foreach(var p in table) {
                    if(p.handValue(0) == 21) {
                        var c = Character.get_character(p.player_discord_id);
                        c.balance += p.bet;
                        Character.update_character(c);
                        toOutput.Add(p.name + " has a blackjack! This results in a __push__.");
                    } else if(p.insurance > 0) {
                        var c = Character.get_character(p.player_discord_id);
                        c.balance += 3*p.insurance;
                        Character.update_character(c);
                        toOutput.Add(p.name + " wins their insurance bet and receives " + 3*p.insurance + " credits back.");
                    }
                }
            } else {
                var dealerValue = handValue(hand);
                foreach(var p in table) {
                    if(p.surrender) continue;
                    for(int i = 0; i < p.hand.Count; i++) {
                        if(p.handValue(i) > 21) {
                            toOutput.Add(p.name +"s hand " + (i+1) + " is a bust");
                        } else if (p.handValue(i) == 21 && p.hand[i].Count == 2) {
                            var c = Character.get_character(p.player_discord_id);
                            c.balance += (int)((double)p.bet*1.5 + p.bet);
                            Character.update_character(c);
                            toOutput.Add(p.name + " has a blackjack with hand " + (i+1) + "! They win " + (int)((double)p.bet*1.5 + p.bet) + " credits.");
                        } else if (dealerValue < 22) {
                            if (p.handValue(i) == dealerValue) {
                                var c = Character.get_character(p.player_discord_id);
                                if(p.doubleDown[i]) {
                                    c.balance += p.bet*2;
                                } else {
                                    c.balance += p.bet;
                                }
                                Character.update_character(c);
                                toOutput.Add(p.name + " has a tie with hand " + (i+1) + "! This results in a __push__.");
                            } else if (p.handValue(i) > dealerValue) {
                                var c = Character.get_character(p.player_discord_id);
                                if(p.doubleDown[i]) {
                                    c.balance += p.bet*4;
                                    toOutput.Add(p.name + " beats the dealer with hand " + (i+1) + "! They win " + 4*p.bet + " credits.");
                                } else {
                                    c.balance += p.bet*2;
                                    toOutput.Add(p.name + " beats the dealer with hand " + (i+1) + "! They win " + 2*p.bet + " credits.");
                                }
                                Character.update_character(c);    
                            } else {
                                toOutput.Add(p.name + " loses to the dealer with hand " + (i+1) + "!");
                            }
                        } else {
                            var c = Character.get_character(p.player_discord_id);
                            if(p.doubleDown[i]) {
                                c.balance += p.bet*4;
                                toOutput.Add(p.name + " beats the dealer with hand " + (i+1) + "! They win " + 4*p.bet + " credits.");
                            } else {
                                c.balance += p.bet*2;
                                toOutput.Add(p.name + " beats the dealer with hand " + (i+1) + "! They win " + 2*p.bet + " credits.");
                            }
                            Character.update_character(c);
                        }
                    }
                }
            }
            toOutput.Add("To start the next round, one player of the game just needs to type `ta!next`");
            resetTable();
            gameRunning = false;
            helpers.output(channel,toOutput);
            toOutput = new List<string>();
        }

        private void resetTable() {
            foreach(var p in table) {
                p.reset();
            }
            hand = new List<StandardCard>();
        }

        public void runGame(SocketCommandContext context = null) {
            if(context != null) {
                var p = table.FirstOrDefault(e=>e.player_discord_id == context.User.Id);
                if(p == null) {
                    helpers.output(channel,context.User.Mention + ", you aren't in the game so you can't start the next round.");
                    return;
                }
            }
            if (gameRunning) {
                helpers.output(channel, context.User.Mention + " a hand is already going!");
                return;
            }
            subPlayer();
            gameRunning = true;
            if(table.Count > 0) {
                toOutput.Add(dealerName + " asks everyone to put forth their wager.");
                collectBets();
                subPlayer();
                if(CardsUntilShuffle <= 0) newDeck();
                dealHand();
                displayTable(true);
                currentRound = table.Count;
                position = new Tuple<int,int>(0,0);
                checkDealer();
            } else {
                toOutput.Add(dealerName + " sits and wait for someone to approach his table.");
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
                CardsUntilShuffle = -1;
            }
        }

        public void takeInsurance(SocketCommandContext context, int insure) {
            var p = table.FirstOrDefault(e=>e.player_discord_id == context.User.Id);
            if(p == null) {
                helpers.output(channel,context.User.Mention + ", you aren't in this blackjack game. To join `ta!join`");
                return;
            }
            if(!gameRunning) {
                helpers.output(channel,context.User.Mention + ", use `ta!next` to start the next round or `ta!join [amount]` to join the game");
                return; 
            }
            var c = Character.get_character(p.player_discord_id);
            if (c == null) {
                helpers.output(channel,context.User.Mention + ", Context Xavier. Something horrible has gone wrong: No Character Account while in Blackjack Game");
                return;
            }
            if (insure < 0) {
                if(c.balance-p.bet/2 >= 0) {
                    p.insurance = p.bet/2;
                    helpers.output(channel,context.User.Mention + ", you have placed an insurance bet of " + p.insurance);
                    c.balance -= p.insurance;
                    Character.update_character(c);
                } else {
                    helpers.output(channel,context.User.Mention + ", you don't have enough money to make this bet of insurance. You place a 0 credit insurance bid.");
                    p.insurance = 0;
                }
            } else {
                if(c.balance-insure >= 0) {
                    if(insure > p.bet/2) insure = p.bet/2;
                    p.insurance = insure;
                    helpers.output(channel,context.User.Mention + ", you have placed an insurance bet of " + p.insurance);
                    c.balance -= p.insurance;
                    Character.update_character(c);
                } else {
                    helpers.output(channel,context.User.Mention + ", you don't have enough money to make this bet of insurance. You place a 0 credit insurance bid.");
                    p.insurance = 0;
                }
            }
            position = new Tuple<int,int>(position.Item1+1,position.Item2);
            if(position.Item1 == currentRound) {
                if(handValue(hand) == 21) {
                    toOutput.Add(dealerName + " has blackjack! Hand: " + hand[0].ToString() + " | " + hand[1].ToString());
                    payouts();
                } else {
                    position = new Tuple<int, int>(0,0);
                    nextPlayer();
                }
            }
        }

        public void hit(SocketCommandContext context) { 
            var p = table[position.Item1];
            if(!gameRunning) {
                helpers.output(channel,context.User.Mention + ", use `ta!next` to start the next round or `ta!join [amount]` to join the game");
                return; 
            }
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            var card = GetCard();
            p.hand[position.Item2].Add(card);
            if(p.handValue(position.Item2) > 21) {
                toOutput.Add(p.name + " busts with a hand of " + p.handDisplay(position.Item2));
                position = new Tuple<int, int>(position.Item1,position.Item2+1);
                if(position.Item2 < p.hand.Count) {
                    toOutput.Add(p.name + " time to play hand " + position.Item2 + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
                    helpers.output(channel,toOutput);
                    toOutput = new List<string>();
                } else {
                    position = new Tuple<int, int>(position.Item1+1,0);
                    nextPlayer();
                }
            } else {
                if(p.handValue(position.Item2) == 21) {
                    toOutput.Add(p.name + " now has a hand of " + p.handDisplay(position.Item2) + System.Environment.NewLine + "A 21 means you automatically stand.");
                    position = new Tuple<int, int>(position.Item1,position.Item2+1);
                    if(position.Item2 < p.hand.Count) {
                        toOutput.Add(p.name + " time to play hand " + position.Item2 + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
                        helpers.output(channel,toOutput);
                        toOutput = new List<string>();
                    } else {
                        position = new Tuple<int, int>(position.Item1+1,0);
                        nextPlayer();
                        return;
                    }
                }
                toOutput.Add(p.name + " now has a hand of " + p.handDisplay(position.Item2) + System.Environment.NewLine + "It's still your turn.");
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
            }
        }

        public void stand(SocketCommandContext context) {
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            if(!gameRunning) {
                helpers.output(channel,context.User.Mention + ", use `ta!next` to start the next round or `ta!join [amount]` to join the game");
                return; 
            }
            toOutput.Add(p.name + " stands.");
            position = new Tuple<int, int>(position.Item1,position.Item2+1);
            if(position.Item2 < p.hand.Count) {
                toOutput.Add(p.name + " time to play hand " + (position.Item2+1) + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
            } else {
                position = new Tuple<int, int>(position.Item1+1,0);
                nextPlayer();
            }
        }

        public void doubleDown(SocketCommandContext context) {
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            if(!gameRunning) {
                helpers.output(channel,context.User.Mention + ", use `ta!next` to start the next round or `ta!join [amount]` to join the game");
                return; 
            }
            if (p.hand[position.Item2].Count != 2) {
                helpers.output(channel,context.User.Mention + ", Sorry you can only double down as the first move on a hand.");
                return;
            }
            var c = Character.get_character(p.player_discord_id);
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", Contact Xavier. Error Code: Character missing while in Blackjack Game");
                return;
            }
            if(c.balance-p.bet <= 0) {
                helpers.output(channel,context.User.Mention + ", You don't have enough in the bank to make this bet");
                return;
            }
            c.balance -= p.bet;
            var card = GetCard();
            p.hand[position.Item2].Add(card);
            p.doubleDown[position.Item2] = true;
            toOutput.Add(p.name + " doubles down. They are dealt " + card.ToString() + " and have a total hand of " + System.Environment.NewLine + p.handDisplay(position.Item2));
            
            position = new Tuple<int, int>(position.Item1,position.Item2+1);
            if(position.Item2 < p.hand.Count) {
                toOutput.Add(p.name + " time to play hand " + (position.Item2+1) + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
            } else {
                position = new Tuple<int, int>(position.Item1+1,0);
                nextPlayer();
            }
        }

        public void split(SocketCommandContext context) {
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            if(!gameRunning) {
                helpers.output(channel,context.User.Mention + ", use `ta!next` to start the next round or `ta!join [amount]` to join the game");
                return; 
            }
            if(p.hand.Count == 4) {
                helpers.output(channel,context.User.Mention + ", sorry. You can only split upto a maximum of 4 hands.");
                return;
            }
            var val1 = p.hand[position.Item2][0].value;
            if(val1 != 14 && val1 > 10) val1 = 10;
            var val2 = p.hand[position.Item2][1].value;
            if(val2 != 14 && val2 > 10) val2 = 10;
            if(val1 == val2) {
                var c = Character.get_character(p.player_discord_id);
                if (c == null) {
                    helpers.output(channel,context.User.Mention + ", Contact Xavier: Error Blackjack without Character");
                    return;
                }
                if (c.balance - p.bet < 0) {
                    helpers.output(channel,context.User.Mention + ", you don't have the money to split your hand");
                    return;
                }
                c.balance -= p.bet;
                Character.update_character(c);
                p.hand.Add(new List<StandardCard>());
                p.doubleDown.Add(false);
                p.hand[p.hand.Count-1].Add(p.hand[position.Item2][1]);
                p.hand[position.Item2].RemoveAt(1);
                p.hand[p.hand.Count-1].Add(GetCard());
                p.hand[position.Item2].Add(GetCard());
                toOutput.Add(p.name + " splits their hand. There current hands are " + p.handDisplay() + System.Environment.NewLine + "You are new resolving hand " + (position.Item2+1));
                helpers.output(channel,toOutput);
                toOutput = new List<string>();
            }
        }

        public void surrender(SocketCommandContext context) {
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            if(!gameRunning) {
                helpers.output(channel,context.User.Mention + ", use `ta!next` to start the next round or `ta!join [amount]` to join the game");
                return; 
            }
            if(p.hand.Count == 1 && p.hand[0].Count == 2) {
                var c  = Character.get_character(p.player_discord_id);
                if (c == null) {
                    helpers.output(channel,context.User.Mention + ", Contact Xavier: Error Blackjack without Character");
                    return;
                }
                c.balance += p.bet/2;
                p.surrender = true;
                toOutput.Add(p.name + " surrenders their hand receving " + p.bet/2 + " credits back.");
            }

            position = new Tuple<int, int>(position.Item1+1,0);
            nextPlayer();
        }
    }

}