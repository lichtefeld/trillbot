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

    public partial class Card {
        [JsonProperty ("ID")]
        public int ID { get; set; }
        [JsonProperty ("count")]
        public int count { get; set; }

        [JsonProperty ("title")]
        public string title { get; set; }

        [JsonProperty ("description")]
        public string description { get; set; }

        [JsonProperty ("type")]
        public string type { get; set; }

        [JsonProperty ("Value")]
        public long value { get; set; }
    }
    public partial class Card
    {
        public static Card[] FromJson(string json) => JsonConvert.DeserializeObject<Card[]>(json, Converter.Settings);

        public override string ToString() {
            return "**" + title + "** : " + description;
        }

        public static List<Card> get_card () {
            var store = new DataStore ("card.json");

            // Get employee collection
            var rtrner = store.GetCollection<Card> ().AsQueryable ().ToList();
            store.Dispose();
            return rtrner;
        }

        public static Card get_card (int id) {
            var store = new DataStore ("card.json");

            // Get employee collection
            var rtrner = store.GetCollection<Card> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
            store.Dispose();
            return rtrner;
        }

        public static Card get_card (string name) {
            var store = new DataStore ("card.json");

            // Get employee collection
            var rtrner = store.GetCollection<Card> ().AsQueryable ().FirstOrDefault (e => e.title == name);
            store.Dispose();
            return rtrner;
        }

        public static void insert_card (Card card) {
            var store = new DataStore ("card.json");

            // Get employee collection
            store.GetCollection<Card> ().InsertOneAsync (card);

            store.Dispose();
        }

        public static void update_card (Card card) {
            var store = new DataStore ("card.json");

            store.GetCollection<Card> ().ReplaceOneAsync (e => e.ID == card.ID, card);
            store.Dispose();
        }

        public static void delete_card (Card card) {
            var store = new DataStore ("card.json");

            store.GetCollection<Card> ().DeleteOne (e => e.ID == card.ID);
            store.Dispose();
        }
    }

}