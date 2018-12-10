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
        public async Task enableBlackjackAsync(string name, int i, int minBet, int maxBet) {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value != null) {
                await Context.Channel.SendMessageAsync("Woah there, a blackjack dealer is already initialized in this channel.");
                return;
            }
            var game = new blackjackDealer(name,i,Context.Channel,minBet,maxBet);
            Program.blackjack.Add(Context.Channel.Id, game);
            await Context.Channel.SendMessageAsync("Blackjack Dealer Added");
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

        /* [Command("join")]
        public async Task joinGameAsync(int b) {
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
            if(b < bj.Value.minbet || b > bj.Value.maxbet) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry this table has a minimum bet of " + bj.Value.minbet + " and a maximum bet of " + bj.Value.maxbet + ". You must bet between those values.");
                return;
            }
            if(bj.Value.table.FirstOrDefault(e=>e.player_discord_id == Context.User.Id) != null) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry you have already joined this blackjack table.");
                return;
            }
            var p = new blackjackPlayer(Context.User.Id,c.name,b);
            bj.Value.addPlayer(p,Context);
        }

        [Command("leave")]
        public async Task leaveBlackjackAsync() {
            var bj = Program.blackjack.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (bj.Value == null) {
                await Context.Channel.SendMessageAsync("Woah there, isn't blackjack dealer in this channel.");
                return;
            }
            bj.Value.subPlayer(Context);
        }*/

    }
}
