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
        public List<Classes.Bet> bets { get; set; }

        public static List<Character> get_character () {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<Character> ().AsQueryable ().ToList();
        }

        public static Character get_character (int id) {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<Character> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
        }

        public static Character get_character (string name) {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<Character> ().AsQueryable ().FirstOrDefault (e => e.name == name);
        }

        public static Character get_character (ulong player_id) {
            var store = new DataStore ("character.json");

            // Get employee collection
            return store.GetCollection<Character> ().AsQueryable ().FirstOrDefault (e => e.player_discord_id == player_id);
        }

        public static void insert_character (Character character) {
            var store = new DataStore ("character.json");

            // Get employee collection
            store.GetCollection<Character> ().InsertOneAsync (character);

            store.Dispose();
        }

        public static void update_character (Character character) {
            var store = new DataStore ("character.json");

            store.GetCollection<Character> ().UpdateOne (e => e.ID == character.ID, character);
            store.Dispose();
        }

        public static void delete_character (Character character) {
            var store = new DataStore ("character.json");

            store.GetCollection<Character> ().DeleteOne (e => e.ID == character.ID);
            store.Dispose();
        }

    }

}