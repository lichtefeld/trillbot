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
        private int position { get; set; }
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
                p.addCard(GetCard());
                p.addCard(GetCard());
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

        private void nextPlayer() {
            var usr = channel.GetUserAsync(table[position].player_discord_id).GetAwaiter().GetResult();
            helpers.output(channel,usr.Mention + ", it is now your turn. " + System.Environment.NewLine + table[position].handDisplay() + System.Environment.NewLine + table[position].name + ", would you like to hit, ");
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
            }
        }

        public void runGame() {
            subPlayer();
            if(table.Count > 0) {
                helpers.output(channel,dealerName + " asks everyone to put forth their wager.");
                collectBets();
                subPlayer();
                if(CardsUntilShuffle <= 0) newDeck();
                dealHand();
                displayTable(true);
                currentRound = table.Count-1; 
                position = 0;
                checkDealer();
            } else {
                helpers.output(channel,dealerName + " sits and wait for someone to approach his table.");
                CardsUntilShuffle = -1;
            }
        }

    }

}