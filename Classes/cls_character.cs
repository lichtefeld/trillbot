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

    public class Character {
        public int ID { get; set; }
        public ulong player_discord_id { get; set; }
        public string name { get; set; }
        public int balance { get; set; } = 1000000;
        public List<Classes.Bet> bets { get; set; } = new List<Bet>();

        public static List<Character> get_character () {
            var store = new DataStore ("character.json");

            // Get employee collection
            var rtrner = store.GetCollection<Character> ().AsQueryable ().ToList();
            store.Dispose();
            return rtrner;
        }

        public static Character get_character (int id) {
            var store = new DataStore ("character.json");

            // Get employee collection
            var rtrner = store.GetCollection<Character> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
            store.Dispose();
            return rtrner;
        }

        public static Character get_character (string name) {
            var store = new DataStore ("character.json");

            // Get employee collection
            var rtrner = store.GetCollection<Character> ().AsQueryable ().FirstOrDefault (e => e.name == name);
            store.Dispose();
            return rtrner;
        }

        public static Character get_character (ulong player_id) {
            var store = new DataStore ("character.json");

            // Get employee collection
            var rtrner = store.GetCollection<Character> ().AsQueryable ().FirstOrDefault (e => e.player_discord_id == player_id);
            store.Dispose();
            return rtrner;
        }

        public static void insert_character (Character character) {
            var store = new DataStore ("character.json");

            // Get employee collection
            store.GetCollection<Character> ().InsertOneAsync (character);

            store.Dispose();
        }

        public static void update_character (Character character) {
            var store = new DataStore ("character.json");

            store.GetCollection<Character> ().ReplaceOneAsync (e => e.ID == character.ID, character);
            store.Dispose();
        }

        public static void delete_character (Character character) {
            var store = new DataStore ("character.json");

            store.GetCollection<Character> ().DeleteOne (e => e.ID == character.ID);
            store.Dispose();
        }

    }

}