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

    public partial class psiball_Team {
        [JsonProperty ("ID")]
        public int ID { get; set; }
        [JsonProperty("Title")]
        public string title { get; set; }
        [JsonProperty("Descrption")]
        public string description { get; set; }
        [JsonProperty("PlayerIDs")]
        public List<ulong> players { get; set; }
    }
    public partial class psiball_Team
    {
        public static psiball_Team[] FromJson(string json) => JsonConvert.DeserializeObject<psiball_Team[]>(json, Converter.Settings);

        public static List<psiball_Team> get_psiball_Team () {
            var store = new DataStore ("psiball_Team.json");

            // Get employee collection
            var rtrner = store.GetCollection<psiball_Team> ().AsQueryable ().ToList();
            store.Dispose();
            return rtrner;
        }

        public static psiball_Team get_psiball_Team (int id) {
            var store = new DataStore ("psiball_Team.json");

            // Get employee collection
            var rtrner = store.GetCollection<psiball_Team> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
            store.Dispose();
            return rtrner;
        }

        public static psiball_Team get_psiball_Team (string name) {
            var store = new DataStore ("psiball_Team.json");

            // Get employee collection
            var rtrner = store.GetCollection<psiball_Team> ().AsQueryable ().FirstOrDefault (e => e.title == name);
            store.Dispose();
            return rtrner;
        }

        public static void insert_psiball_Team (psiball_Team psiball_Team) {
            var store = new DataStore ("psiball_Team.json");

            // Get employee collection
            store.GetCollection<psiball_Team> ().InsertOneAsync (psiball_Team);

            store.Dispose();
        }

        public static void update_psiball_Team (psiball_Team psiball_Team) {
            var store = new DataStore ("psiball_Team.json");

            store.GetCollection<psiball_Team> ().ReplaceOneAsync (e => e.ID == psiball_Team.ID, psiball_Team);
            store.Dispose();
        }

        public static void delete_psiball_Team (psiball_Team psiball_Team) {
            var store = new DataStore ("psiball_Team.json");

            store.GetCollection<psiball_Team> ().DeleteOne (e => e.ID == psiball_Team.ID);
            store.Dispose();
        }
    }

}