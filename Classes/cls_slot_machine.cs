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

    public partial class slotMachine {

        [JsonProperty("Id")]
        public int ID {get; set; }
        [JsonProperty("Name")]
        public string name { get; set; }
        [JsonProperty("Description")]
        public string description { get; set; }
        [JsonProperty("Max Bet")]
        public int maxBet {get; set; }
        [JsonProperty("Min Bet")]
        public int minBet {get; set; }
        [JsonProperty("Reels")]
        public List<string> reels { get; set; } = new List<string>();
        [JsonProperty("Weights")]
        public List<List<int>> Weights { get; set; } = new List<List<int>>();
        [JsonProperty("Payouts")]
        public List<int> Payouts { get; set; } = new List<int>();
        [JsonProperty("Channel")]
        public ulong ChannelID {get; set;}

        private string displayReel(int i, int j, int k ) {
            if (i < 0 || i > reels.Count || j < 0 || j > reels.Count || k < 0 || k > reels.Count) {
                return "";
            } else {
                return reels[i] + " | " + reels[j] + " | " + reels[k];
            }
        }

        public string rollSlot(Character c, int bet, ICommandContext Context) {
            List<string> str = new List<string>();
            str.Add("**" + this.name + "**");
            int[] rolls = new int[3];
            for(int i = 0; i < rolls.Length; i++) {
                var roll = Program.rand.Next(1,65);
                int sum = 0;
                for(int j = 0; j < Weights[i].Count; j++) {
                    sum += Weights[i][j];
                    if (roll <= sum) {
                        rolls[i] = j;
                        break;
                    }
                }
            }
            str.Add(displayReel(rolls[0], rolls[1], rolls[2])); //Comments Assume Default Emotes
            if(rolls[0] == 0 && rolls[1] == 0 && rolls[2] == 0) { //3 Moneybags
                str.Add("JACKPOT! You've Won " + Payouts[0]*bet + " Credits!");
                c.balance+=Payouts[0]*bet;
            } else if (rolls[0] == 1 || rolls[0] == 2 || rolls[0] == 3 ) { //One of each Fruit
                switch(rolls[0]) {
                    case 3:
                    if(rolls[1] == 2) {
                        if(rolls[2] == 1) {
                            str.Add("You've Won " + Payouts[1]*bet + " Credits!");
                            c.balance+=Payouts[1]*bet;
                        }
                    } else if (rolls[1] == 1) {
                        if(rolls[2] == 2) {
                            str.Add("You've Won " + Payouts[1]*bet + " Credits!");
                            c.balance+=Payouts[1]*bet;
                        }
                    }
                    break;
                    case 2:
                    if(rolls[1] == 1) {
                        if(rolls[2] == 3) {
                            str.Add("You've Won " + Payouts[1]*bet + " Credits!");
                            c.balance+=Payouts[1]*bet;
                        }
                    } else if (rolls[1] == 3) {
                        if(rolls[2] == 1) {
                            str.Add("You've Won " + Payouts[1]*bet + " Credits!");
                            c.balance+=Payouts[1]*bet;
                        }
                    }
                    break;
                    case 1:
                    if(rolls[1] == 2) {
                        if(rolls[2] == 3) {
                            str.Add("You've Won " + Payouts[1]*bet + " Credits!");
                            c.balance+=Payouts[1]*bet;
                        }
                    } else if (rolls[1] == 3) {
                        if(rolls[2] == 2) {
                            str.Add("You've Won " + Payouts[1]*bet + " Credits!");
                            c.balance+=Payouts[1]*bet;
                        }
                    }
                    break;
                } 
            } else if (rolls[0] == 1 && rolls[1] == 1 && rolls[2] == 1) { //Three Cherries
                str.Add("You've Won " + Payouts[2]*bet + " Credits!");
                c.balance+=Payouts[2]*bet;
            } else if (rolls[0] == 2 && rolls[1] == 2 && rolls[2] == 2) { //Three Grapes
                str.Add("You've Won " + Payouts[3]*bet + " Credits!");
                c.balance+=Payouts[3]*bet;
            } else if (rolls[0] == 3 && rolls[1] == 3 && rolls[2] == 3) { //Three Lemons
                str.Add("You've Won " + Payouts[4]*bet + " Credits!");
                c.balance+=Payouts[4]*bet;
            } else if ((rolls[0] == 1 || rolls[0] == 2 || rolls[0] == 3) && (rolls[1] == 1 || rolls[1] == 2 || rolls[1] == 3) && (rolls[2] == 1 || rolls[2] == 2 || rolls[2] == 3)) { //Three Fruits
                str.Add("You've Won " + Payouts[5]*bet + " Credits!");
                c.balance+=Payouts[5]*bet;
            } else if (rolls[0] == 4 && rolls[1] == 4 && rolls[2] == 4) { //Three Rose
                str.Add("You've Won " + Payouts[6]*bet + " Credits!");
                c.balance+=Payouts[6]*bet;
            } else if (rolls[0] == 5 && rolls[1] == 5 && rolls[2] == 5) {//Three Sunfowers
                str.Add("You've Won " + Payouts[7]*bet + " Credits!");
                c.balance+=Payouts[7]*bet;
            } else if(rolls[0] == 6 && rolls[1] == 6 && rolls[2] == 6) { //Three Hibiscous 
                str.Add("You've Won " + Payouts[8]*bet + " Credits!");
                c.balance+=Payouts[8]*bet;
            } else if ((rolls[0] == 4 || rolls[0] == 5 || rolls[0] == 6) && (rolls[1] == 4 || rolls[1] == 5 || rolls[1] == 6) && (rolls[2] == 4 || rolls[2] == 5 || rolls[2] == 6)) {//Three Flowers
                str.Add("You've Won " + Payouts[9]*bet + " Credits!");
                c.balance+=Payouts[9]*bet;
            } else if (rolls[0] != 7 && rolls[1] != 7 && rolls[2] != 7) { //Three non-pinapple symbols
                str.Add("You've Won " + Payouts[10]*bet + " Credits!");
                c.balance+=Payouts[10]*bet;
            } else if (rolls[0] == 7 && rolls[1] == 7 && rolls[2] == 7) {
                str.Add("You lost " + bet + " credits");
                if(ChannelID != 0) security(Context);
            } else {
                str.Add("You lost " + bet + " credits");
            }
            return(String.Join(System.Environment.NewLine,str));
        }
        public string payouts() {
            List<string> str = new List<string>();
            str.Add("**" + this.name + " Payouts**");
            str.Add(this.description);
            str.Add("*Payouts assume a bet of 1 Imperial Credit.* This machines's max bet is: " + this.maxBet + ". This machine's minimum bet is: " + this.minBet);
            str.Add(reels[0] + " | " + reels[0] + " | " + reels[0] + ": " + Payouts[0]);
            str.Add(reels[1] + " | " + reels[2] + " | " + reels[3] + ": " + Payouts[1]);
            str.Add(reels[1] + " | " + reels[1] + " | " + reels[1] + ": " + Payouts[2]);
            str.Add(reels[2] + " | " + reels[2] + " | " + reels[2] + ": " + Payouts[3]);
            str.Add(reels[3] + " | " + reels[3] + " | " + reels[3] + ": " + Payouts[4]);
            str.Add(reels[1] + reels[2] + reels[3] + " | " + reels[1] + reels[2] + reels[3] + " | " + reels[1] + reels[2] + reels[3] + ": " + Payouts[5]);
            str.Add(reels[4] + " | " + reels[4] + " | " + reels[4] + ": " + Payouts[6]);
            str.Add(reels[5] + " | " + reels[5] + " | " + reels[5] + ": " + Payouts[7]);
            str.Add(reels[6] + " | " + reels[6] + " | " + reels[6] + ": " + Payouts[8]);
            str.Add(reels[4] + reels[5] + reels[6] + " | " + reels[4] + reels[5] + reels[6] + " | " + reels[4] + reels[5] + reels[6] + ": " + Payouts[9]);
            str.Add("Any three symbols which aren't " + reels[7] + ": " + Payouts[10]);
            return(String.Join(System.Environment.NewLine,str));
        }

        public void security(ICommandContext Context) { 
            var channel = Context.Guild.GetChannelAsync(ChannelID).GetAwaiter().GetResult() as ISocketMessageChannel;
            var usrs = Context.Guild.GetUsersAsync().GetAwaiter().GetResult().ToList();
            var user = usrs[Program.rand.Next(usrs.Count)];

            string output = user.Nickname.ToString() + " is being monitored by security drones.";
            helpers.output(channel, output);
        }

    }

    public partial class slotMachine {
        public static slotMachine[] FromJson(string json) => JsonConvert.DeserializeObject<slotMachine[]>(json, Converter.Settings);

        public static List<slotMachine> get_slotMachine () {
            var store = new DataStore ("slotMachine.json");

            // Get employee collection
            var rtrner = store.GetCollection<slotMachine> ().AsQueryable ().ToList();
            store.Dispose();
            return rtrner;
        }

        public static slotMachine get_slotMachine (int id) {
            var store = new DataStore ("slotMachine.json");

            // Get employee collection
            var rtrner = store.GetCollection<slotMachine> ().AsQueryable ().FirstOrDefault (e => e.ID == id);
            store.Dispose();
            return rtrner;
        }

        public static slotMachine get_slotMachine (string name) {
            var store = new DataStore ("slotMachine.json");

            // Get employee collection
            var rtrner = store.GetCollection<slotMachine> ().AsQueryable ().FirstOrDefault (e => e.name == name);
            store.Dispose();
            return rtrner;
        }

        public static void insert_slotMachine (slotMachine slotMachine) {
            var store = new DataStore ("slotMachine.json");

            // Get employee collection
            store.GetCollection<slotMachine> ().InsertOneAsync (slotMachine);

            store.Dispose();
        }

        public static void update_slotMachine (slotMachine slotMachine) {
            var store = new DataStore ("slotMachine.json");

            store.GetCollection<slotMachine> ().ReplaceOneAsync (e => e.ID == slotMachine.ID, slotMachine);
            store.Dispose();
        }

        public static void delete_slotMachine (slotMachine slotMachine) {
            var store = new DataStore ("slotMachine.json");

            store.GetCollection<slotMachine> ().DeleteOne (e => e.ID == slotMachine.ID);
            store.Dispose();
        }
    }

}