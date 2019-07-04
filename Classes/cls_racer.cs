using System;
using System.Collections.Generic;
using System.Linq;
using JsonFlatFileDataStore;

namespace trillbot.Classes {

    public class racer {
        public int ID { get; set; }
        public ulong player_discord_id { get; set; }
        public ulong server_discord_id { get ; set; }
        public string name { get; set; }
        public string faction { get; set; }
        public string descr { get; set; }
        public string img { get; set; }
        //Variables Used for Game Mechanics:
        public List<Classes.Card> cards { get; set; } = new List<Classes.Card>();
        public long distance { get; set;} 
        public List<pair> hazards { get; set; } = new List<pair>();
        public bool inGame { get; set; } = false;
        public bool stillIn { get; set; } = true;
        public bool crash { get; set; } = false;
        public Ability ability = Ability.get_ability("ability.json",1);
        public bool abilityRemaining = true;
        public racer coreSync = null;

        public void addHazard(Card card) {
            var h = hazards.FirstOrDefault(e=>e.item1.ID == card.ID);
            if (h == null) {
                hazards.Add(new pair(card,0));
            }
        }
        public void addHazard(Card card, int i) {
            var h = hazards.FirstOrDefault(e=>e.item1.ID == card.ID);
            if (h == null) {
                hazards.Add(new pair(card,i));
            }
        }
        public bool canMove() {
            var h = hazards.FirstOrDefault(e=> e.item1.ID == 5 || e.item1.ID == 8 || e.item1.ID == 6 || e.item1.ID == 16);
            return h == null;
        }

        public bool maxMove2() {
            var h = hazards.FirstOrDefault(e=> e.item1.ID == 11);
            return h == null;
        }

        public bool sab() {
            var h = hazards.FirstOrDefault(e=> e.item1.ID == 9);
            return h == null;
        }

        public void reset() {
            stillIn = true;
            crash = false;
            distance = 0;
            hazards = new List<pair>();
            cards = new List<Classes.Card>();
            inGame = false;
            abilityRemaining = true;
        }

        public string nameID() {
            return this.name + " (" + this.ID + ")";
        }

        public string leader(int[] lengths, bool odd, bool activePlayer, textVersion tV) {
            var str = new List<string>();
            if(lengths == null) {
                if (activePlayer) str.Add("/* " + this.twoDigitDistance());
                else if (odd) str.Add("# " + this.twoDigitDistance());
                else str.Add("> " + this.twoDigitDistance());
                str.Add(this.nameID());
                str.Add(tV.leaderBoardAlive(this.stillIn));
                str.Add(this.faction);
                str.Add(this.ability.Title);
                foreach(pair p in hazards) {
                    str.Add(p.item1.title + " (" + (p.item2+1) + ")");
                }
                if (activePlayer) str.Add(" *");
            } else {
                if (activePlayer) str.Add("/*      " + this.twoDigitDistance());
                else if (odd) str.Add("#       " + this.twoDigitDistance());
                else str.Add(">       " + this.twoDigitDistance());
                str.Add(helpers.center(this.nameID(),lengths[0]));
                str.Add(helpers.center(tV.leaderBoardAlive(this.stillIn),lengths[1]));
                str.Add(helpers.center(this.faction,lengths[2]));
                str.Add(helpers.center(this.ability.Title,lengths[3]));
                foreach (pair p in hazards) {
                    str.Add(p.item1.title + " (" + (p.item2+1) + ")");
                }
                if (activePlayer) str.Add(" *");
            }
            var output_string = String.Join(" | ", str);
                return output_string;
        }

        private string twoDigitDistance() {
            if (-1 < this.distance && this.distance < 10) {
                return "0"+ this.distance.ToString();
            } else {
                return distance.ToString();
            }
        }

        public string currentStatus(textVersion tV) {
            var str2 = new List<string>();
            str2.Add(this.name + "'s Hand");
            str2.Add("-- -- -- -- -- -- -- -- -- --");
            //Special Ability
            var active = "Passive";
            if (this.ability.Active){
                active = "Active";
            }
            str2.Add("**Special Ability:** " + this.ability.Title + " (" + active + ") - " + this.ability.Description);
            str2.Add("Ability Use Remaining: " + this.abilityRemaining);
            //Cards
           str2.Add("**Current Cards**");
            if (this.cards.Count == 0) { 
                str2.Add("No Cards");
            } else {
                for(int i = 0; i < this.cards.Count; i++) {
                    str2.Add("#" + (i+1) + ": " + this.cards[i].ToString());
                }
            }
            //Hazards
            str2.Add("--");
            str2.Add(tV.statusHazard());
            if (this.hazards.Count == 0) str2.Add("None");
            var j = 0;
            foreach (pair p in this.hazards) {
                str2.Add("#" + ++j + ": " + p.item1.title +" has been applied for " + (p.item2+1) + " turns. " + tV.id_to_condition[p.item1.ID]);
            }
            str2.Add("-- -- -- -- -- -- -- -- -- --");
            return String.Join(System.Environment.NewLine, str2);
        }

        public static List<racer> get_racer () {
            var store = new DataStore ("racer.json");

            var rtner = store.GetCollection<racer> ().AsQueryable ().ToList();

            store.Dispose();

            // Get employee collection
            return rtner;
        }

        public static racer get_racer (int id) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            var rtner = store.GetCollection<racer> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
            store.Dispose();

            // Get employee collection
            return rtner;
        }

        public static racer get_racer (string name) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            var rtner = store.GetCollection<racer> ().AsQueryable ().FirstOrDefault (e => e.name == name);
            store.Dispose();

            // Get employee collection
            return rtner;
        }

        public static racer get_racer (ulong player_id, ulong server_id) {
            var store = new DataStore ("racer.json");

            // Get employee collection
            var rtner = store.GetCollection<racer> ().AsQueryable ().FirstOrDefault (e => e.player_discord_id == player_id && e.server_discord_id == server_id);
            store.Dispose();

            // Get employee collection
            return rtner;
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