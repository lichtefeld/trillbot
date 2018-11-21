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

    public class roulettePlayer {
        public ulong player_discord_id { get; set; }
        public string name { get; set; }
        public List<rouletteBet> bets { get; set; } = new List<rouletteBet>();

        public roulettePlayer(ulong id, string n) {
            player_discord_id = id;
            name = n;
        }

        public bool reset() {
            var b = new List<rouletteBet>();
            var c = Character.get_character(player_discord_id);
            foreach (var bet in bets) {
                if(bet.rolling) {
                    if(c.balance - bet.amount >= 0) {
                        b.Add(bet);
                        c.balance -= bet.amount;
                    }
                }    
            }
            bets = b;
            Character.update_character(c);
            if(bets.Count > 0) return true;
            else return false;
        }
    }

    public class rouletteBet {
        public string type { get; set; }
        public List<int> nums { get; set; } = new List<int>();
        public int amount { get; set;}
        public bool leftOption { get; set;}
        public bool rolling { get; set; } //For having a bet last through rounds, triggers automatic next game. Currently defaulting to false
         /*
        Low/High
        Red/Black
        Evens/Odds */

        public rouletteBet(string t, int a, bool left = false, List<int> num = null) {
            type = t;
            amount = a;
            leftOption = left;
            nums = num;
            rolling = false;
        }

        public override string ToString() {
            switch(type) {
                case "straight":
                case "split": 
                case "street":
                case "square":
                case "line":
                case "dozens":
                case "columns":
                    return type + " on " + String.Join(" | ",nums) + " for " + amount;
                case "color":
                    if(leftOption) {
                        return type + " on Red for " + amount;
                    } else {
                        return type + " on Black for " + amount;
                    }
                case "half":
                    if(leftOption) {
                        return type + " on Low for " + amount;
                    } else {
                        return type + " on High for " + amount;
                    }
                case "pair":
                    if(leftOption) {
                        return type + " on Evens for " + amount;
                    } else {
                        return type + " on Odds for " + amount;
                    }
            }
            return "";
        }
       
    }

    public class roulette {
        private static readonly List<int> reds = new List<int> {1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36};
        private static readonly List<int> blacks = new List<int> {2, 4, 6, 8, 10, 11, 13, 15, 17, 20, 22, 24, 26, 28, 29, 31, 33, 35};
        public static readonly List<string> betTypes = new List<string> {"straight", "split", "street", "square", "line", "color", "dozens", "columns", "half", "pair"};

        //Internal Variables
        public string dealerName { get; set; }
        private ISocketMessageChannel channel { get; set;}
        public List<roulettePlayer> table { get; set; } = new List<roulettePlayer>();
        private List<ulong> toSub { get; set; } = new List<ulong>();
        private bool isRolling { get; set; }
        private rouletteTimer Timer { get; set; }
        public int minBet { get; set; }
        public int maxInside {get; set;}
        public int maxOutside { get; set;}
        public roulette(string dN, ISocketMessageChannel chan, int min, int inside, int outside) {
            dealerName = dN;
            channel = chan;
            minBet = min;
            maxInside = inside;
            maxOutside = outside;
            Timer = new rouletteTimer(chan,this);
        }

        public void config() {
            //To Build Config Display
            channel.SendFileAsync("roulette.jpg","").GetAwaiter().GetResult();
        }

        public void join(SocketCommandContext context, roulettePlayer p) {
            table.Add(p);
            helpers.output(channel,context.User.Mention + ", has joined the roulette table with " + dealerName + " as the table host.");
        }

        public void leave(SocketCommandContext context) {
            var player = table.FirstOrDefault(e=> e.player_discord_id == context.User.Id);
            if(player != null) {
                if(isRolling) {
                    toSub.Add(context.User.Id);
                    helpers.output(channel,context.User.Mention + ", you nod and will leave this table at the end of the round.");
                } else {
                    table.Remove(player);
                    helpers.output(channel,context.User.Mention + ", you stand up and leave the table.");
                }
            }
        }

        private void subPlayer() {
            foreach(var i in toSub) {
                table.Remove(table.FirstOrDefault(e=> e.player_discord_id == i));
            }
            toSub = new List<ulong>();
        }

        public void bet(SocketCommandContext context, rouletteBet bet) {
            var p = table.FirstOrDefault(e=> e.player_discord_id == context.User.Id);
            if (p == null ) {
                helpers.output(channel,context.User.Mention + ", you aren't at this roulette table. To join `ta!join`");
                return;
            }
            p.bets.Add(bet);
            helpers.output(channel,context.User.Mention + " you have placed a bet: " + bet.ToString());
            if(!isRolling) {
                isRolling = true;
                Timer.startTimer();
            }
        }

        public void payouts(int num) {
            bool rollAgain = false;
            var str = new List<string>();
            //Formatting of Roulette Wheel Output
            str.Add("```diff");
            str.Add("The wheel stops and the ball begins to roll what slot will it stop at? ");
            if(reds.Contains(num)) str.Add("- The Number is " + num + "```");
            else if (blacks.Contains(num)) str.Add("--- The Number is " + num + "```");
            else if (num ==37) str.Add("+ The Number is 00```");
            else str.Add("+ The Number is " + num  + "```");

            //Adding Each Player's Successful Bets
            foreach(var p in table) {
                var c = Character.get_character(p.player_discord_id);
                foreach(var b in p.bets) {
                    var payout = 0;
                    switch(b.type) {
                        case "straight":
                            if (b.nums[0] == num) {
                                payout = b.amount + b.amount * 35; 
                            }
                        break;
                        case "split": 
                            if(b.nums.Contains(num)) {
                                payout = b.amount + b.amount * 17;
                            }
                        break;
                        case "street":
                            if(b.nums.Contains(num)) {
                                payout = b.amount + b.amount * 11;
                            }
                        break;
                        case "square":
                            if(b.nums.Contains(num)) {
                                payout = b.amount + b.amount * 8;
                            }
                        break;
                        case "line":
                            if(b.nums.Contains(num)) {
                                payout = b.amount + b.amount * 5;
                            }
                        break;
                        case "color":
                            if(b.leftOption) {
                                if(reds.Contains(num)) payout = b.amount + b.amount;
                            } else {
                                if(blacks.Contains(num)) payout = b.amount + b.amount;
                            }
                        break;
                        case "dozens":
                            if(b.nums.Contains(num)) {
                                payout = b.amount + b.amount * 2;
                            }
                        break;
                        case "columns":
                            if(b.nums.Contains(num)) {
                                payout = b.amount + b.amount * 2;
                            }
                        break;
                        case "half":
                            if(b.leftOption) {
                                if(num != 0 && num < 19) payout = b.amount + b.amount;
                            } else {
                                if(num != 37 && num > 18) payout = b.amount + b.amount;
                            }
                        break;
                        case "pair":
                            if(b.leftOption) {
                                if(num != 0 && num % 2 == 0) payout = b.amount + b.amount;
                            } else {
                                if(num != 37 && num % 2 == 1) payout = b.amount + b.amount;
                            }
                        break;
                    }
                    if (payout > 0) {
                        c.balance += payout;
                        str.Add(p.name + " has a successful " + b.type + " bet on space " + num + " paying out " + payout + ".");
                    }
                }
                Character.update_character(c);
                if(p.reset()) rollAgain = true;
            }
            helpers.output(channel,str);
            subPlayer();
            if(rollAgain) Timer.startTimer();
            else isRolling = false;
        }
    }

}