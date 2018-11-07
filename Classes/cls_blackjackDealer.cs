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

        //List of Players to Add, List of Players to Remove

        public blackjackDealer(string name, int numDecks) {
            dealerName = name;
            numberOfDecks = numDecks;
            newDeck();
        }  

        private void newDeck() {
            List<StandardCard> cards = StandardCard.straightDeck();
            List<StandardCard> multiCard = cards;
            for(int i = 1; i <= numberOfDecks; i++) {
                multiCard.AddRange(cards);
            }
            deck = StandardCard.shuffleDeck(cards);
        }

        public void addPlayer() {

        }

        public void subPlayer() {

        }

    }

}