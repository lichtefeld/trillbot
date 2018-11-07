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

    public class StandardCard {

        //
        public int value { get; set; }
        public int suit { get; set; }

        public StandardCard(int v, int s) {
            value = v;
            suit = s;
        }

        public static readonly Dictionary<int,string> value_to_output = new Dictionary<int, string>() {
            {2,"2"},
            {3,"3"},
            {4,"4"},
            {5,"5"},
            {6,"6"},
            {7,"7"},
            {8,"8"},
            {9,"9"},
            {10,"10"},
            {11, "J"},
            {12,"Q"},
            {13,"K"},
            {14,"A"}
        };

        public static readonly Dictionary<int,string> suit_to_output = new Dictionary<int, string>() {
            {1,"♤"},
            {2,"♧"},
            {3,"♡"},
            {4,"♢"}
        };

        public static List<StandardCard> straightDeck() {
            List<StandardCard> cards = new List<StandardCard>();
            for(int i = 1; i < 5; i++) {
                for(int k = 2; k < 15; k++) {
                    cards.Add(new StandardCard(k, i));
                }
            }
            return cards;
        }

        public static Stack<StandardCard> shuffleDeck(List<StandardCard> deck) {
            Stack<StandardCard> shuffled = new Stack<StandardCard>();
            while(deck.Count > 0) {
                shuffled.Append(deck[Program.rand.Next(deck.Count)]);
            }
            return shuffled;
        }
    }
}