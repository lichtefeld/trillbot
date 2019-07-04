using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class Roulette : ModuleBase<SocketCommandContext>
    {
        [Command("enableroulette")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task enableRouletteAsync(string name, int minBet, int maxInside, int maxOutside) {
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value != null) {
                await Context.Channel.SendMessageAsync("Woah there, a roulette table is already initialized in this channel.");
                return;
            }
            var game = new roulette(name,Context.Channel,minBet,maxInside,maxOutside);
            Program.roulette.Add(Context.Channel.Id, game);
            await Context.Channel.SendMessageAsync("Roulette Table Added");
        }

        [Command("removeroulette")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task removeBlackjackAsync() {
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't roulette table in this channel.");
                return;
            }
            Program.roulette.Remove(Context.Channel.Id);
            await Context.Channel.SendMessageAsync("Dealer removed.");
        }

        /*[Command("join")]
        public async Task joinRouletteAsync() {
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't roulette table in this channel.");
                return;
            }
            var c = Character.get_character(Context.User.Id);
            if (c == null) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you haven't created an account `ta!registeraccount`.");
                return;
            }
            if(rl.Value.table.FirstOrDefault(e=>e.player_discord_id == Context.User.Id) != null) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry you have already joined this blackjack table.");
                return;
            }
            var p = new roulettePlayer(c.player_discord_id,c.name);
            rl.Value.join(Context,p);
        }
        [Command("leave")]
        public async Task leaveRouletteAsync() {
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't roulette table in this channel.");
                return;
            }
            rl.Value.leave(Context);
        }*/

        [Command("bet")]
        public async Task roulleteBet(params string[] inputs) {
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value == null) {
                //await Context.Channel.SendMessageAsync("Woah there, isn't roulette table in this channel.");
                return;
            }

            var c = Character.get_character(Context.User.Id,Context.Guild.Id);
            if (c == null) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you haven't created an account `ta!registeraccount`.");
                return;
            }

            var p = rl.Value.table.FirstOrDefault(e=> e.player_discord_id == Context.User.Id);
            if (p == null ) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't at this roulette table. To join `ta!join`");
                return;
            }

            if(inputs.Length < 3) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", your input ins't structured correctly: `ta!bet type [amount] [Nums OR Type Option]`");
                return;
            }

            var type = inputs[0].ToLower();
            if(!roulette.betTypes.Contains(type)) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid type. Please select one of: `" + String.Join("` | `",roulette.betTypes) + "`");
                return;
            }
            
            int amount = 0;
            Int32.TryParse(inputs[1], out amount);
            if (amount < 1) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid amount. Please provide an amount greater than 0");
                return;
            }
            
            if(c.balance - amount < 0) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you don't have the money to make this bet.");
                return;
            }
            var num = new List<int>();
            int i;
            bool left = false;
            switch(type) {
                case "straight":
                    Int32.TryParse(inputs[2], out i);
                    if (i < 0 || i > 37) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid board space. Please provide a space between 0 and 37 where 37 = 00.");
                        return;
                    }
                    num.Add(i);
                break;
                case "split":
                    if (inputs.Length != 4) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", your input ins't structured correctly: `ta!bet split " + amount + " [Num 1] [Num 2]`");
                        return;
                    }
                    Int32.TryParse(inputs[2], out i);
                    if (i < 0 || i > 37) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid board space. Please provide a space between 0 and 37 where 37 = 00.");
                        return;
                    }
                    num.Add(i);
                    for(int j = 3; j < inputs.Length; j++) {
                        Int32.TryParse(inputs[j], out i);
                        if (i < 0 || i > 37) {
                            await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid board space. Please provide a space between 0 and 37 where 37 = 00.");
                            return;
                        }
                        if (num[0] - 1 != i && num[0] + 1 != i && num[0] - 3 != i && num[0] + 3 != i && num[0] != i) { //Check not valid
                            await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid board space to split with. `ta!bet split " + amount + " " + num[0] + " [Num 2]`");
                            return;
                        }
                        num.Add(i);
                    }
                break;
                case "street":
                    Int32.TryParse(inputs[2], out i);
                    if (i < 1 || i > 12) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid street number. Please provide a number between 1 - 12. `ta!bet street " + amount + " [1-12]` ");
                        return;
                    }
                    for(int j = 0; j < 3; j++) {
                        num.Add(1+3*(i-1)+j);
                    }
                break;
                case "square":
                    await ReplyAsync("Currently not accepting Square bets");
                    return;
                    //TO BUILD
                break;
                case "line":
                    Int32.TryParse(inputs[2], out i);
                    if (i < 1 || i > 11) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid line number. Please provide a number between 1 - 11. `ta!bet line " + amount + " [1-11]` ");
                        return;
                    }
                    for(int j = 0; j < 6; j++) {
                        num.Add(1+3*(i-1)+j);
                    }      
                break;
                case "dozens":
                    Int32.TryParse(inputs[2], out i);
                    if (i < 1 || i > 3) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid dozen number. Please provide a number between 1 - 3. `ta!bet dozen " + amount + " [1-3]` ");
                        return;
                    }
                    for(int j = 0; j < 12; j++) {
                        num.Add(1+12*(i-1)+j);
                    }
                break;
                case "columns":
                    Int32.TryParse(inputs[2], out i);
                    if (i < 1 || i > 3) {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid column number. Please provide a number between 1 - 3. `ta!bet column " + amount + " [1-3]` ");
                        return;
                    }
                    for(int j = 0; j < 12; j++) {
                        num.Add(i+j*3);
                    }
                break;
                case "color":
                    if(inputs[2].ToLower().Equals("red")) {
                        left = true;
                    } else if (inputs[2].ToLower().Equals("black")) {
                        left = false;
                    } else {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid color. Please provide either `red` or `black`. `ta!bet color " + amount + " [red / black]`");
                        return;
                    }
                break;
                case "half":
                    if(inputs[2].ToLower().Equals("low")) {
                        left = true;
                    } else if (inputs[2].ToLower().Equals("high")) {
                        left = false;
                    } else {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid half. Please provide either `low` or `high`. `ta!bet half " + amount + " [low / high]`");
                        return;
                    }
                break;
                case "pair":
                    if(inputs[2].ToLower().Equals("even")) {
                        left = true;
                    } else if (inputs[2].ToLower().Equals("odd")) {
                        left = false;
                    } else {
                        await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid pair. Please provide either `even` or `odd`. `ta!bet pair " + amount + " [even / odd]`");
                        return;
                    }
                break;
            }
            var b = new rouletteBet(type,amount,left,num);
            if(inputs[inputs.Count() - 1].ToLower() == "true") b.rolling = true;
            c.balance -= amount;
            rl.Value.bet(Context,b);
        }
    }
}