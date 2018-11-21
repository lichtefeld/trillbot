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

    public class blackjackPlayer {
        public ulong player_discord_id { get; set; }
        public string name { get; set; }
        public List<List<StandardCard>> hand { get; set; } = new List<List<StandardCard>>();
        public int bet { get; set; }
        public int insurance { get; set;}
        public List<bool> doubleDown { get; set;} = new List<bool>();
        public bool surrender { get; set;}

        public blackjackPlayer(ulong ID, string n, int b) {
            player_discord_id = ID;
            name = n;
            bet = b;
            doubleDown.Add(false);
            surrender = false;
            hand.Add(new List<StandardCard>());
        }

        public void reset() {
            hand = new List<List<StandardCard>>();
            hand.Add(new List<StandardCard>());
            doubleDown = new List<bool>();
            doubleDown.Add(false);
            surrender = false;
        }

        public int handValue(int i = 0) {
            if (i < 0 || i >= hand.Count) return -1;
            else return handValue(hand[i]);
        }

        private int handValue(List<StandardCard> cards) {
            int value = 0;
            int numAces = 0;
            if (cards.Count == 0) return -1;
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

        public string handDisplay() {
            List<string> str = new List<string>();
            str.Add("**" + name + "'s Hands**");
            for(int i = 0; i < hand.Count; i++) {
                str.Add("*Hand " + (i+1) + "*");
                str.Add(handDisplay(hand[i]));
            }
            return String.Join(System.Environment.NewLine,str);
        }

        public string handDisplay(int i) {
            List<string> str = new List<string>();
            str.Add("**" + name + "'s Hand: " + (i+1) + "**");
            str.Add(handDisplay(hand[i]));
            return String.Join(System.Environment.NewLine,str);
        }
        private string handDisplay(List<StandardCard> cards) {
            List<string> str = new List<string>();
            if (cards.Count == 0) return "";
            foreach(var c in cards) {
                str.Add(c.ToString());
            }
            str.Add("`" + handValue(cards).ToString() + "`");
            return String.Join(" | ", str);
        }

    }
}