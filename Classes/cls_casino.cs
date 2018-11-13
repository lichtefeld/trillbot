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

    public class casino_BJ {
        public string dealerName { get; set; }
        public int minBet { get; set; }
        public int maxBet { get; set; }
        public int numDecks { get; set; }
        public ulong chanID { get; set; }

        //Constructor
        public casino_BJ(Classes.blackjackDealer dealer) {
            dealerName = dealer.dealerName;
            minBet = dealer.minbet;
            maxBet = dealer.maxbet;
            numDecks = dealer.numberOfDecks;
            chanID = dealer.channel.Id;
        }

        //Functions
        public List<string> display(SocketGuild Guild) {
            var chan = Guild.GetTextChannel(chanID);
            var str = new List<string>();
            str.Add(dealerName + " is located in " + chan);
            str.Add("Using " + numDecks + " decks with a minimum bet of " + minBet + " and a maximum bet of " + maxBet);
            return str;
        }
    }

    public class casino_Slot {
        public int ID { get; set; }
        public string name { get; set; }
        public ulong chanID { get; set;}

        public casino_Slot(Classes.slotMachine sm) {
            ID = sm.ID;
            name = sm.name;
            //chanID = sm
        }

        public string display(SocketGuild Guild) {
            var chan = Guild.GetTextChannel(chanID);
            return name + " slots bound to " + chan;
        }
    }

    public partial class Casino {
        //Variables
        [JsonProperty("ID")]
        public int Id { get; set; }
        [JsonProperty("Name")]
        public string name { get; set; }
        [JsonProperty("blackjacks")]
        public List<casino_BJ> blackjacks { get; set; } = new List<casino_BJ>();
        [JsonProperty("slots")]
        public List<casino_Slot> slots { get; set; } = new List<casino_Slot>();
        [JsonProperty("Guild ID")]
        public ulong guildID { get; set;}
        [JsonProperty("Casino Admin Channels")]
        public List<ulong> adminChannels { get; set; } = new List<ulong>();

        //Constructor
        public Casino() {

        }
        //Methods
        public void rebuild(SocketCommandContext Context) {
            //Permissions Check rather than on the external level
            if(!isCasinoManager(Context.Guild.GetUser(Context.User.Id))) {
                helpers.output(Context.Channel,Context.User.Mention + " you aren't an administrator nor do you have the `Casino Manager` role.");
            }
            //Rebuild Blackjack
            blackjacks.ForEach(e=> {
                if ( trillbot.Program.blackjack.ContainsKey(e.chanID)) {
                    trillbot.Program.blackjack.Remove(e.chanID);
                }
                var chan = Context.Guild.GetTextChannel(e.chanID);
                if (chan == null) {
                    Context.User.SendMessageAsync("Failed to create dealer with name " + e.dealerName + ". Did you delete the channel?");
                    return;
                }
                var bj = new blackjackDealer(e.dealerName,e.numDecks,chan,e.minBet,e.maxBet);
                trillbot.Program.blackjack.Add(e.chanID,bj);
                helpers.output(Context.Channel,"A dealer with the name of " + bj.dealerName + " has been added to this channel.");
            });
            //To repeat above code for each list.
        }

        public void save() {
            Casino.update_Casino(this);
        }

        public void display(SocketCommandContext Context) {
            if(!isCasinoManager(Context.Guild.GetUser(Context.User.Id))) {
                helpers.output(Context.User,Context.User.Mention + ", sorry you aren't a casino admin. If you should be please contact your server manager.");
                return;
            }
            if(!isCasinoAdminChannel(Context.Channel)) {
                helpers.output(Context.User,Context.User.Mention + ", " + Context.Channel + " is not an admin channel for the casino. To add it please type `ta!casino admin`");
                return;
            }

            var str = new List<string>();
            str.Add(this.name + " bound to Server " + Context.Guild.Name);
            str.Add("**BLACKJACK TABLES**"); //Blackjack List
            foreach (var b in blackjacks) {
                str.AddRange(b.display(Context.Guild));
            }
            str.Add("**SLOT MACHINES**"); //Slot Machine Lists
            foreach (var sm in slots) {
                str.Add(sm.display(Context.Guild));
            }

            //TO FILL IN ADDITIONAL GAMES
            str.Add("**ADMIN CHANNELS**"); //Admin Channel List
            foreach(var id in adminChannels) {
                var chan = Context.Guild.GetTextChannel(id);
                str.Add(chan + " - Admin Channel");
            }

            helpers.output(Context.Channel,str);
        }
        public bool isCasinoAdminChannel(ISocketMessageChannel Channel) {
            if (adminChannels.Count == 0) return true;
            if (adminChannels.Contains(Channel.Id)) return true;
            return false;
        }

        //Static Methods
        public static bool isCasinoManager(SocketGuildUser User) {
            if (User.Id == 106768024857501696) return true; //Giving Xavier Automatic access to admin commands
            var roles = User.Roles;
            var manager = roles.FirstOrDefault(e=>e.Name == "Casino Manager"); //Check for 'Casino Manager' roll
            if (manager != null) return true;
            else {
                manager = roles.FirstOrDefault(e=>e.Permissions.Administrator);
                if (manager != null) return true;
                else return false;
            }
        }


    }

    public partial class Casino
    {
        public static Casino[] FromJson(string json) => JsonConvert.DeserializeObject<Casino[]>(json, Converter.Settings);


        public static List<Casino> get_Casino () {
            var store = new DataStore ("Casino.json");

            // Get employee collection
            var rtrner = store.GetCollection<Casino> ().AsQueryable ().ToList();
            store.Dispose();
            return rtrner;
        }

        public static Casino get_Casino (int id) {
            var store = new DataStore ("Casino.json");

            // Get employee collection
            var rtrner = store.GetCollection<Casino> ().AsQueryable ().FirstOrDefault (e => e.Id == id);
            store.Dispose();
            return rtrner;
        }

        public static void insert_Casino (Casino Casino) {
            var store = new DataStore ("Casino.json");

            // Get employee collection
            store.GetCollection<Casino> ().InsertOneAsync (Casino);

            store.Dispose();
        }

        public static void update_Casino (Casino Casino) {
            var store = new DataStore ("Casino.json");

            store.GetCollection<Casino> ().ReplaceOneAsync (e => e.Id == Casino.Id, Casino);
            store.Dispose();
        }

        public static void delete_Casino (Casino Casino) {
            var store = new DataStore ("Casino.json");

            store.GetCollection<Casino> ().DeleteOne (e => e.Id == Casino.Id);
            store.Dispose();
        }
    }

}