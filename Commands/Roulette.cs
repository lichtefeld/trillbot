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

        [Command("join")]
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
        }

        [Command("bet")]
        public async Task roulleteBet(params string[] inputs) {
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
            var type = inputs[0].ToLower();
            if(!roulette.betTypes.Contains(type)) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you didn't provide a valid type. Please select one of: `" + String.Join("` | `",roulette.betTypes) + "`");
                return;
            }
            

        }
    }
}