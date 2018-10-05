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
            str.Add(this.stillIn.ToString());
            str.Add(this.faction);
            int[] haz = {-1, -1, -1, -1, -1, -1, -1};
            hazards.ForEach(e=> {
                haz[e.item1.ID-5] = e.item2;
            });
            foreach (int b in haz) {
                str.Add(b.ToString());
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