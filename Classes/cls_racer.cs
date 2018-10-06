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

    public class racer {
        public int ID { get; set; }
        public ulong player_discord_id { get; set; }
        public string name { get; set; }
        public string faction { get; set; }
        public List<Classes.Card> cards { get; set; } = new List<Classes.Card>();
        //Variables Used for Game Mechanics:
        public long distance { get; set;} 
        public List<pair> hazards { get; set; } = new List<pair>();
        public bool stillIn { get; set; } = true;
        public bool crash { get; set; } = false;
        public bool canMove { get; set; } = true;
        public bool maxMove2 { get; set; } = false;
        public bool sab { get; set; } = false;

        public void reset() {
            stillIn = true;
            canMove = true;
            crash = false;
            maxMove2 = false;
            sab = false;
            distance = 0;
            hazards = new List<pair>();
            cards = new List<Classes.Card>();
        }

        public string nameID() {
            return this.name + " (" + this.ID + ")";
        }

        public string leader() {
            List<string> str = new List<string>();
            str.Add(this.twoDigitDistance());
            str.Add(this.nameID());
            if(this.stillIn) {
                str.Add("Alive");
            } else {
                str.Add("Dead");
            }
            str.Add(this.faction);
            foreach(pair p in hazards) {
                str.Add(p.item1.title + " (" + (p.item2+1) + ")");
            }
            string output_string = String.Join(" | ", str);
            return output_string;
        }

        private string twoDigitDistance() {
            if( this.distance < 10 ) {
                return "0"+ this.distance;
            } else {
                return distance.ToString();
            }
        }

        public string currentStatus() {
            List<string> str2 = new List<string>();
                str2.Add("**Your Current Hand**");
                if (this.cards.Count == 0) { 
                    return "Hold up, you don't have any cards. The game must not have started yet.";
                } else {
                    for(int i = 0; i < this.cards.Count; i++) {
                        str2.Add("#" + (i+1) + ": " + this.cards[i].ToString());
                    }
                    str2.Add("-- -- -- -- --");
                    str2.Add("**Current Hazards** - If any Hazard is applied for 3 turns, you will explode.");
                    if (this.hazards.Count == 0) str2.Add("None");
                    int j = 0;
                    foreach (pair p in this.hazards) {
                        str2.Add("#" + ++j + ": " + p.item1.title +" has been applied for " + p.item2 + " turns. " + id_to_condition[p.item1.ID]);
                    }
                    return String.Join(System.Environment.NewLine, str2);
                    
                }
        }

        private Dictionary<int, string> id_to_condition = new Dictionary<int, string> {
            {5,"You cannot move until you play a Dodge card."},
            {6, "You cannot move until you play a Dodge card."},
            {8, "You cannot move until you play a Tech Savvy card."},
            {9, "Can be removed by a Tech Savvy card. If you end your turn with both Sabotage and another Hazard, you explode."},
            {10, "Can be removed by a Cyber Healthcare card."},
            {11, "You cannot play Movement cards higher than 2. Can be removed by a Cyber Healthcare card."},
            {16, "You can not move this turn. Does not need a remedy to clear."}
        };

        public static List<racer> get_racer () {
            var store = new DataStore ("racer.json");

            // Get employee collection
            return store.GetCollection<racer> ().AsQueryable ().ToList();
        }

        public static racer get_racer (int id) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            return store.GetCollection<racer> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
        }

        public static racer get_racer (string name) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            return store.GetCollection<racer> ().AsQueryable ().FirstOrDefault (e => e.name == name);
        }

        public static racer get_racer (ulong player_id) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            return store.GetCollection<racer> ().AsQueryable ().FirstOrDefault (e => e.player_discord_id == player_id);
        }

        public static void insert_racer (racer racer) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            store.GetCollection<racer> ().InsertOneAsync (racer);

            store.Dispose();
        }

        public static void update_racer (racer racer) {
            var store = new DataStore ("racer.json");

            store.GetCollection<racer> ().ReplaceOne (e => e.ID == racer.ID, racer);
            store.Dispose();
        }

        public static void replace_racer(racer racer) {
            var store = new DataStore ("racer.json");

            store.GetCollection<racer> ().ReplaceOne (e => e.ID == racer.ID, racer);
            store.Dispose();
        }

        public static void delete_racer (racer racer) {
            var store = new DataStore ("racer.json");

            store.GetCollection<racer> ().DeleteOne (e => e.ID == racer.ID);
            store.Dispose();
        }

    }

}