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
        public List<Tuple<int,int>> hazards { get; set; } = new List<Tuple<int,int>>();
        public bool stillIn { get; set; } = false;
        public bool crash { get; set; } = false;
        public bool canMove { get; set; } = true;
        public bool maxMove2 { get; set; } = false;
        public bool heartAtt { get; set; } = false;
        public int heartTurns { get; set; } = 0;
        public bool sab { get; set; } = false;

        public void reset() {
            stillIn = false;
            canMove = true;
            crash = false;
            maxMove2 = false;
            heartAtt = false;
            sab = false;
            heartTurns = 0;
            distance = 0;
            //reset hazards
            cards = new List<Classes.Card>();
        }

        public string nameID() {
            return this.name + " (" + this.ID + ") ";
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