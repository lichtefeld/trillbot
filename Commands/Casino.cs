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
    public class Casino : ModuleBase<SocketCommandContext> {
        //ADMIN COMMANDS
        [Command("casino")]
        public async Task casinoNestedCommands(params string[] inputs) {
            if(inputs.Length == 0) {
                var casino = Classes.Casino.get_Casino(Context.Guild.Id);
                if (casino == null) {
                    await ReplyAsync("No casino for this server found... attempting to create one");
                    makeCasino(Context);
                    return;
                }
                casino.display(Context);
                return;
            }
        }

        public void makeCasino(SocketCommandContext Context) {
            if(Classes.Casino.isCasinoManager(Context.Guild.GetUser(Context.User.Id))) {
                var c = new Classes.Casino(Context.Guild);
                helpers.output(Context.Channel,Context.User.Mention + ", Casino generated for this server");
                Classes.Casino.insert_Casino(c);
            }
        }

        //GROUP CASINO GAME COMMANDS
        [Command("payouts")] //List Game Payouts, if a game doesn't exist in that channel, fail silently. If no other output, DM user to let them know no game exists in that channel
        public async Task payoutsAsync() {
            //Slots
            var slot = Program.slots.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (slot.Value != null) {
                await ReplyAsync(slot.Value.payouts());
                return;
            }

            //Blackjack
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value != null) {
                await ReplyAsync(bj.Value.payout());
                return;
            }

            //Roulette
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value != null) {
                await ReplyAsync(rl.Value.payouts());
                return;
            }
            
            //Deafult Output
            await Context.User.SendMessageAsync("Sorry, no game exists in " + Context.Channel + ". Contact an Admin or Casino Manager if you believe this to be incorrect");
        }

        [Command("join")]
        public async Task joinGameAsync(params string[] inputs) {
            var c = Character.get_character(Context.User.Id);
            if (c == null) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you haven't created an account `ta!registeraccount`.");
                return;
            }

            //If Blackjack Table?
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value != null) {
                if (inputs.Length != 1) {
                    await Context.Channel.SendMessageAsync(Context.User.Mention + ", I'm sorry. You must provide the bet per round at this blackjack table. `ta!join [" + bj.Value.minbet + " to " + bj.Value.maxbet + "]`");
                    return;
                }

                //Try to Parse Bet Amount
                int b = 0;
                Int32.TryParse(inputs[0],out b); 
                if(b == 0) {
                    await Context.Channel.SendMessageAsync(Context.User.Mention + ", I'm sorry. You must provide the bet per round at this blackjack table. `ta!join [" + bj.Value.minbet + " to " + bj.Value.maxbet + "]`");
                    return;
                }
                //Check Bet Amount to be within proper values
                if(b < bj.Value.minbet || b > bj.Value.maxbet) {
                    await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry this table has a minimum bet of " + bj.Value.minbet + " and a maximum bet of " + bj.Value.maxbet + ". You must bet between those values.");
                    return;
                }
                //Check if Player is already at the table. 
                if(bj.Value.table.FirstOrDefault(e=>e.player_discord_id == Context.User.Id) != null) {
                    await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry you have already joined this blackjack table.");
                    return;
                }
                var p = new blackjackPlayer(Context.User.Id,c.name,b);
                bj.Value.addPlayer(p,Context); //Add New Blackjack player to to the table
            }

            //If Roulette Table
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if(rl.Value != null) {
                //Check if Player in game
                if(rl.Value.table.FirstOrDefault(e=>e.player_discord_id == Context.User.Id) != null) {
                    await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry you have already joined this blackjack table.");
                    return;
                }

                var p = new roulettePlayer(c.player_discord_id,c.name);
                rl.Value.join(Context,p);
            }

            //Default Response
            await Context.User.SendMessageAsync("I'm sorry. There isn't a game to join in " + Context.Channel);
        }

        [Command("leave")]
        public async Task leaveGameAsync() {
            //Blackjack
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value != null) {
                bj.Value.subPlayer(Context);
                return;
            }

            //Roulette
            var rl = Program.roulette.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (rl.Value != null) {
                rl.Value.leave(Context);
                return;
            }

            //Default Output
            await Context.User.SendMessageAsync("There isn't a game to leave in " + Context.Channel);

        }

        //SLOT MACHINE SPECIFIC COMMANDS
        [Command("slot")]
        public async Task slotAsync(int bet = 1) {
            var slot = Program.slots.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (slot.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't slot machine in " + Context.Channel);
                return;
            }
            
            slot.Value.rollSlot(bet, Context);
        }

        //BLACKJACK SPECIFIC COMMANDS
        [Command("blackjack")]
        public async Task explainBlackjackAsync() {
            await Context.User.SendMessageAsync(blackjackDealer.explain());
        }

        [Command("hit")] //Standard Backjack Option
        public async Task hitBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.hit(Context);
        }

        [Command("stand")] //Standard Backjack Option
        public async Task standBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.stand(Context);
        }

        [Command("double")] //Standard Backjack Option
        public async Task doubleBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.doubleDown(Context);
        }

        [Command("split")] //Standard Backjack Option
        public async Task splitBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.split(Context);
        }

        [Command("surrender")] //Standard Backjack Option
        public async Task surrenderBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.surrender(Context);
        }

        [Command("insurance")] //When the dealer offers insurance, use this command to bet on it. Should consider adding a timer which moves forward even if not all players have indicated yes or no?
        public async Task insuranceBlackjackAsync(int i = -1) {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.takeInsurance(Context, i);
        }

        [Command("next")] //Starts the next hand of BJ (Didn't want to use a timer to automatically start aiming to be event driven not timer based)
        public async Task nextBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.User.SendMessageAsync("Woah there, isn't blackjack dealer in " + Context.Channel);
                return;
            }
            bj.Value.runGame(Context);
        }

        //ROULETTE SPECIFIC COMMANDS
    }

}