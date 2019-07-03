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

    public class casino_BJ { //Temp Storage Unit for a Slot Machine
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
            str.Add("Blackjack Dealer " + dealerName + " is located in " + chan);
            str.Add("Using " + numDecks + " decks with a minimum bet of " + minBet + " and a maximum bet of " + maxBet);
            return str;
        }
    }

    public class casino_RL { //Temp Storage Unit for a Roulette Table
        public string name { get; set;}
        public ulong chanID { get; set;}
        public int minBet { get; set; }
        public int maxInside { get; set;}
        public int maxOutside { get; set;}

        public casino_RL(roulette rl) {
            name = rl.dealerName;
            chanID = rl.channel.Id;
            minBet = rl.minBet;
            maxInside = rl.maxInside;
            maxOutside = rl.maxOutside;
        }

        public List<string> display(SocketGuild Guild) {
            var chan = Guild.GetTextChannel(chanID);
            var str = new List<string>();
            str.Add("Roulette Table with " + name + " as the host. Located in " + chan);
            str.Add("Minimum Bet: " + minBet + ". Maximum Inside: " + maxInside + ". Maxiumum Outside: " + maxOutside);
            return str;
        }
    }

    public partial class Casino {
        //Variables
        [JsonProperty("ID")]
        public int Id { get; set; }
        [JsonProperty("Name")]
        public string name { get; set; }
        [JsonProperty("blackjacks")]
        public List<casino_BJ> blackjacks { get; set; } 
        [JsonProperty("slots")]
        public List<slotMachine> slots { get; set; }
        [JsonProperty("roulettes")]
        public List<casino_RL> roulettes { get; set;}
        [JsonProperty("Guild ID")]
        public ulong guildID { get; set;}
        [JsonProperty("Casino Admin Channels")]
        public List<ulong> adminChannels { get; set; }
        [JsonProperty("Guild ID")]
        public bool autorebuild { get; set; }

        //Constructor
        public Casino(SocketGuild Guild, bool ar = true) {
            guildID = Guild.Id;
            name = Guild.Name;
            autorebuild = ar;
            blackjacks = new List<casino_BJ>();
            slots = new List<slotMachine>();
            roulettes = new List<casino_RL>();
            adminChannels = new List<ulong>();
        }
        //Methods
        public void rebuild(SocketCommandContext Context = null) {
            //Permissions Check rather than on the external level
            if(Context != null && !isCasinoManager(Context.Guild.GetUser(Context.User.Id))) {
                helpers.output(Context.Channel,Context.User.Mention + " you aren't an administrator nor do you have the `Casino Manager` role.");
                return;
            }
            var toUser = new List<string>();
            var toAdminChan = new List<string>();
            //Rebuild Blackjack
            toAdminChan.Add("**BLACKJACK CHANNELS**");
            blackjacks.ForEach(e=> {
                if ( trillbot.Program.blackjack.ContainsKey(e.chanID)) { //Check if blackjack already exists. If so, remove the current dealer and replace with a new one. (In case of stuck dealer)
                    trillbot.Program.blackjack.Remove(e.chanID);
                }
                var chan = Context.Guild.GetTextChannel(e.chanID);
                if (chan == null) {
                    toUser.Add("Failed to create dealer with name " + e.dealerName + " in channel ID: " + e.chanID + ". Did you delete the channel?");
                } else {
                    var bj = new blackjackDealer(e.dealerName,e.numDecks,chan,e.minBet,e.maxBet);
                    trillbot.Program.blackjack.Add(e.chanID,bj);
                    toAdminChan.Add("A dealer with the name of " + bj.dealerName + " has been added to channel " + chan + ".");
                }
            }); //To repeat above code for each list.

            //Rebuild Slots
            var sms = slotMachine.get_slotMachine(); // Fetch List of all Valid Slot Machines
            slots.ForEach(e=> {
                if ( trillbot.Program.slots.ContainsKey(e.ChannelID)) { //Check if blackjack already exists. If so, remove the current dealer and replace with a new one. (In case of stuck dealer)
                    trillbot.Program.slots.Remove(e.ChannelID);
                }
                var chan = Context.Guild.GetTextChannel(e.ChannelID);
                if (chan == null) {
                    toUser.Add("Failed to create slot machine with name " + e.name + " in channel ID: " + e.ChannelID + ". Did you delete the channel?");
                } else {
                    var sm = sms.FirstOrDefault(k=>e.ID == k.ID);
                    if(sm == null) {
                        toUser.Add("Failed to create slot machine with name " + e.name + " in channel " + chan + ". Was the slot machine ID changed?");
                    } else {
                        var smr = new slotMachineRunner(chan, sm);
                        trillbot.Program.slots.Add(e.ChannelID,smr);
                        toAdminChan.Add("A slot machine with the name of " + smr.name + " has been added to channel " + chan + ".");
                    }
                }
            });//To repeat above code for each list.
            
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
            str.Add(this.name + " bound to Server " + this.name);
            str.Add("**BLACKJACK TABLES**"); //Blackjack List
            foreach (var b in blackjacks) {
                str.AddRange(b.display(Context.Guild));
            }
            str.Add("**SLOT MACHINES**"); //Slot Machine Lists
            foreach (var sm in slots) {
                str.Add(sm.display(Context.Guild));
            }
            str.Add("**ROULETTE TABLES**"); //Roulette Tables
            foreach (var rl in roulettes) {
                str.AddRange(rl.display(Context.Guild));
            }

            //TO FILL IN ADDITIONAL GAMES
            str.Add("**ADMIN CHANNELS**"); //Admin Channel List
            foreach(var id in adminChannels) {
                var chan = Context.Guild.GetTextChannel(id);
                str.Add(chan + " - Admin Channel");
            }

            helpers.output(Context.Channel,str);
        }

        public void addRoulette(roulette rl) {
            roulettes.Add(new casino_RL(rl));
        }

        public void addBlackjack(blackjackDealer bj) {
            blackjacks.Add(new casino_BJ(bj));
        }

        public void addAdminChannel(ulong chanID) {
            if(adminChannels.Contains(chanID)) return;
            else {
                adminChannels.Add(chanID);
            }
        }
        public bool isCasinoAdminChannel(ISocketMessageChannel Channel) {
            if (adminChannels.Count == 0) return true;
            return adminChannels.Contains(Channel.Id);
        }

        //Static Methods
        public static bool isCasinoManager(SocketGuildUser User) {
            if (User.Id == 106768024857501696) return true; //Giving Xavier Automatic access to admin commands
            var roles = User.Roles;
            var manager = roles.FirstOrDefault(e=>e.Name == "Casino Manager"); //Check for 'Casino Manager' roll
            if (manager != null) return true;
            else {
                manager = roles.FirstOrDefault(e=>e.Permissions.Administrator);
                return (manager != null);
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

        public static Casino get_Casino (ulong id) {
            var store = new DataStore ("Casino.json");

            // Get employee collection
            var rtrner = store.GetCollection<Casino> ().AsQueryable ().FirstOrDefault (e => e.guildID == id);
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