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

        //List of Players to Add, List of Players to Remove

        public blackjackDealer(string name, int numDecks, ISocketMessageChannel c) {
            dealerName = name;
            numberOfDecks = numDecks;
            channel = c;
            newDeck();
        }  

        private void newDeck() {
            List<StandardCard> cards = StandardCard.straightDeck();
            List<StandardCard> multiCard = cards;
            for(int i = 1; i <= numberOfDecks; i++) {
                multiCard.AddRange(cards);
            }
            CardsUntilShuffle = multiCard.Count/2 + Program.rand.Next(52*(numberOfDecks/2));
            deck = StandardCard.shuffleDeck(cards);
        }

        public void addPlayer(blackjackPlayer p, SocketCommandContext Context) {
            table.Add(p);
            if (!gameRunning) {
                helpers.output(channel,Context.User.Mention + ", you have sat down at " + this.dealerName + "'s table. You'll be betting " + p.bet + " on every hand.");
                runGame();
            } else {
                helpers.output(channel,Context.User.Mention + ", you have sat down at " + this.dealerName + "'s table and will join the next game. You'll be betting " + p.bet + " on every hand.");
            }
        }

        public void subPlayer(SocketCommandContext Context) {
            var player = table.FirstOrDefault(e=> e.player_discord_id == Context.User.Id);
            if(player != null) {
                toLeave.Add(Context.User.Id);
                helpers.output(channel,Context.User.Mention + ", you nod and will leave this table at the end of the round.");
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
                        helpers.output(channel,p.name + " is unable to make this bet and they leave the table.");
                        toLeave.Add(p.player_discord_id);
                    }
                }
            }
        }

        private StandardCard GetCard() {
            CardsUntilShuffle--;
            return deck.Pop();
        }

        private void dealHand() {
            foreach(blackjackPlayer p in table) {
                p.hand[0].Add(GetCard());
                p.hand[0].Add(GetCard());
                p.insurance = -1;
            }
            hand.Add(GetCard());
            hand.Add(GetCard());
        }

        private void displayTable(bool firstRound) {
            List<string> str = new List<string>();
            foreach(blackjackPlayer p in table) {
                var s = p.handDisplay();
                str.Add(s);
                str.Add(System.Environment.NewLine);
            }
            str.Add("**"+dealerName+"'s Hand**");
            str.Add(handDisplay(hand,!firstRound));
            helpers.output(channel,str);
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
            return String.Join(" | ", str);
        }

        private void dealerTurn() {

        }

        private void nextPlayer() {
            if (position.Item1 >= currentRound) {
                dealerTurn();
            } else {
                var usr = channel.GetUserAsync(table[position.Item1].player_discord_id).GetAwaiter().GetResult();
                helpers.output(channel,usr.Mention + ", it is now your turn. " + System.Environment.NewLine + table[position.Item1].handDisplay() + System.Environment.NewLine + table[position.Item1].name + ", would you like to hit, ");
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
                helpers.output(channel,dealerName + " offers insurance to the table.");
                return;
            } else if (handValue(hand) == 21) {
                helpers.output(channel,dealerName + " has blackjack! Hand: " + hand[0].ToString() + " | " + hand[1].ToString());
                payouts();
            } else {
                nextPlayer();
            }
        }

        private void payouts() {
            position = new Tuple<int,int>(0,0);
            var str = new List<string>();
            //Check if Dealer Has Blackjack?
            if(handValue(hand) == 21 && hand.Count == 2) { //Dealer Has Blackjack!
                foreach(var p in table) {
                    if(p.handValue(0) == 21) {
                        var c = Character.get_character(p.player_discord_id);
                        c.balance += p.bet;
                        Character.update_character(c);
                        str.Add(p.name + " has a blackjack! This results in a __push__.");
                    } else if(p.insurance >= 0) {
                        var c = Character.get_character(p.player_discord_id);
                        c.balance += 3*p.insurance;
                        Character.update_character(c);
                        str.Add(p.name + " wins their insurance bet and receives " + 3*p.insurance + " credits back.");
                    }
                }
            } else {
                var dealerValue = handValue(hand);
                foreach(var p in table) {
                    if(p.surrender) continue;
                    for(int i = 0; i < p.hand.Count; i++) {
                        if(p.handValue(i) > 21) {
                            str.Add(p.name +"s hand " + i + " is a bust");
                        } else if (p.handValue(i) == 21 && p.hand[i].Count == 2) {
                            var c = Character.get_character(p.player_discord_id);
                            c.balance += (int)((double)p.bet*1.5 + p.bet);
                            Character.update_character(c);
                            str.Add(p.name + " has a blackjack with hand " + i + "! They win " + (int)((double)p.bet*1.5 + p.bet) + " credits.");
                        } else if (p.handValue(i) == dealerValue) {
                            var c = Character.get_character(p.player_discord_id);
                            if(p.doubleDown) {
                                c.balance += p.bet*2;
                            } else {
                                c.balance += p.bet;
                            }
                            Character.update_character(c);
                            str.Add(p.name + " has a tie with hand " + i + "! This results in a __push__.");
                        } else if (p.handValue(i) > dealerValue) {
                            var c = Character.get_character(p.player_discord_id);
                            if(p.doubleDown) {
                                c.balance += p.bet*4;
                                str.Add(p.name + " beats the dealer with hand " + i + "! They win " + 4*p.bet + " credits.");
                            } else {
                                c.balance += p.bet*2;
                                str.Add(p.name + " beats the dealer with hand " + i + "! They win " + 2*p.bet + " credits.");
                            }
                            Character.update_character(c);    
                        } else {
                            str.Add(p.name + " loses to the dealer with hand " + i + "!");
                        }
                    }
                }
            }
            str.Add("To start the next round, one player of the game just needs to type `ta!next`");
            gameRunning = false;
            helpers.output(channel,str);
        }

        public void runGame() {
            subPlayer();
            gameRunning = true;
            if(table.Count > 0) {
                helpers.output(channel,dealerName + " asks everyone to put forth their wager.");
                collectBets();
                subPlayer();
                if(CardsUntilShuffle <= 0) newDeck();
                dealHand();
                displayTable(true);
                currentRound = table.Count-1; 
                position = new Tuple<int,int>(0,0);
                checkDealer();
            } else {
                helpers.output(channel,dealerName + " sits and wait for someone to approach his table.");
                CardsUntilShuffle = -1;
            }
        }

        public void takeInsurance(SocketCommandContext context, int insure) {
            var p = table.FirstOrDefault(e=>e.player_discord_id == context.User.Id);
            if(p == null) {
                helpers.output(channel,context.User.Mention + ", you aren't in this blackjack game. To join `ta!join`");
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
                    helpers.output(channel,dealerName + " has blackjack! Hand: " + hand[0].ToString() + " | " + hand[1].ToString());
                    payouts();
                } else {
                    nextPlayer();
                }
            }
        }

        public void hit(SocketCommandContext context) { 
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            var card = GetCard();
            p.hand[position.Item2].Add(card);
            if(p.handValue(position.Item2) > 21) {
                helpers.output(channel, p.name + " busts with a hand of " + p.handDisplay(position.Item2));
                position = new Tuple<int, int>(position.Item1,position.Item2+1);
                if(position.Item2 < p.hand.Count) {
                    helpers.output(channel,p.name + " time to play hand " + position.Item2 + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
                } else {
                    position = new Tuple<int, int>(position.Item1+1,0);
                    nextPlayer();
                }
            } else {
                helpers.output(channel, p.name + " now has a hand of " + p.handDisplay(position.Item2) + System.Environment.NewLine + "It's still your turn.");
            }
        }

        public void stand(SocketCommandContext context) {
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
                return;
            }
            helpers.output(channel,p.name + " stands.");
            position = new Tuple<int, int>(position.Item1,position.Item2+1);
            if(position.Item2 < p.hand.Count) {
                helpers.output(channel,p.name + " time to play hand " + position.Item2 + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
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
            p.doubleDown = true;
            helpers.output(channel,p.name + " doubles down. They are dealt " + c.ToString() + " and have a total hand of " + p.handDisplay(position.Item2));
            
            position = new Tuple<int, int>(position.Item1,position.Item2+1);
            if(position.Item2 < p.hand.Count) {
                helpers.output(channel,p.name + " time to play hand " + position.Item2 + ". " + System.Environment.NewLine + p.handDisplay(position.Item2));
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
                p.hand[position.Item2+1].Add(p.hand[position.Item2][1]);
                p.hand[position.Item2].RemoveAt(1);
                p.hand[position.Item2+1].Add(GetCard());
                p.hand[position.Item2].Add(GetCard());
                helpers.output(channel,p.name + " splits their hand. There current hands are " + p.handDisplay() + System.Environment.NewLine + "You are new resolving hand " + position.Item2);
            }
        }

        public void surrender(SocketCommandContext context) {
            var p = table[position.Item1];
            if(p.player_discord_id != context.User.Id) {
                helpers.output(channel,context.User.Mention + ", the dealer isn't interacting with you right now");
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
                helpers.output(channel,p.name + " surrenders their hand receving " + p.bet/2 + " credits back.");
            }
        }
    }

}