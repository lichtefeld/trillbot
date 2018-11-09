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
    public class Blackjack : ModuleBase<SocketCommandContext>
    {
        [Command("enableblackjack")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task enableBlackjackAsync(string name, int i) {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value != null) {
                await Context.Channel.SendMessageAsync("Woah there, a blackjack dealer is already initialized in this channel.");
                return;
            }
            var game = new blackjackDealer(name,i,Context.Channel);
            Program.blackjack.Add(Context.Channel.Id, game);
            await Context.Channel.SendMessageAsync("Slot machine added. Use `ta!payouts` to determin the payouts of this machine.");
        }

        [Command("removeblackjack")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task removeBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            Program.blackjack.Remove(Context.Channel.Id);
            await Context.Channel.SendMessageAsync("Dealer removed.");
        }

        [Command("join")]
        public async Task joinBlackjackAsync(int b) {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            var c = Character.get_character(Context.User.Id);
            if (c == null) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you haven't created an account `ta!registeraccount`.");
                return;
            }
            var p = new blackjackPlayer(Context.User.Id,c.name,b);
            bj.Value.addPlayer(p,Context);
        }

        [Command("leave")]
        public async Task joinBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.subPlayer(Context);
        }

        [Command("hit")]
        public async Task hitBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.hit(Context);
        }

        [Command("stand")]
        public async Task standBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.stand(Context);
        }

        [Command("double")]
        public async Task doubleBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.doubleDown(Context);
        }

        [Command("split")]
        public async Task splitBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.split(Context);
        }

        [Command("surrender")]
        public async Task surrenderBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.surrender(Context);
        }

        [Command("insurance")]
        public async Task insuranceBlackjackAsync(int i = -1) {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.takeInsurance(Context, i);
        }

        [Command("next")]
        public async Task nextBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.runGame();
        }
    }
}
