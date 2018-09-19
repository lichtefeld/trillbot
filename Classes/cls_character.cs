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

    public class character {
        public int ID { get; set; }
        public ulong player_discord_id { get; set; }
        public string name { get; set; }
        public List<Classes.Bet> bets { get; set; }

        public static List<character> get_character () {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<character> ().AsQueryable ().ToList();
        }

        public static character get_character (int id) {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<character> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
        }

        public static character get_character (string name) {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<character> ().AsQueryable ().FirstOrDefault (e => e.name == name);
        }

        public static character get_character (ulong player_id) {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<character> ().AsQueryable ().FirstOrDefault (e => e.player_discord_id == player_id);
        }

        public static void insert_character (character character) {
            var store = new DataStore ("character.json");

            // Get employee collection
            store.GetCollection<character> ().InsertOneAsync (character);

            store.Dispose();
        }

        public static void update_character (character character) {
            var store = new DataStore ("character.json");

            store.GetCollection<character> ().UpdateOne (e => e.ID == character.ID, character);
            store.Dispose();
        }

        public static void delete_character (character character) {
            var store = new DataStore ("character.json");

            store.GetCollection<character> ().DeleteOne (e => e.ID == character.ID);
            store.Dispose();
        }

    }

}