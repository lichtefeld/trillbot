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
    public class AdminCommands : ModuleBase<SocketCommandContext> {
        
        [Command("add")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task addFundsAsync(SocketUser user, int i) {
            var c = Character.get_character(user.Id);
            if(c == null) {
                await ReplyAsync("This user doesn't have an account");
                return;
            }

            if (i <= 0) {
                await ReplyAsync("Don't add negative or 0 funds.");
                return;
            }

            c.balance+=i;
            await ReplyAsync(c.name + " has a new balance of " + c.balance);
            Character.update_character(c);
        }

        [Command("sub")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task subFundsAsync(SocketUser user, int i) {
            var c = Character.get_character(user.Id);
            if(c == null) {
                await ReplyAsync("This user doesn't have an account");
                return;
            }

            if (i <= 0) {
                await ReplyAsync("Don't sub negative or 0 funds.");
                return;
            }

            c.balance-=i;
            await ReplyAsync(c.name + " has a new balance of " + c.balance);
            Character.update_character(c);
        }

        [Command("bal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task balFundsAsync(SocketUser user) {
            var c = Character.get_character(user.Id);
            if(c == null) {
                await ReplyAsync("This user doesn't have an account");
                return;
            }

            await ReplyAsync(c.name + " has a balance of " + c.balance);
            Character.update_character(c);
        }

        [Command("help")]
        public async Task helpAsync() {
            await Context.User.SendMessageAsync("Please check out this google document for my commands: <https://docs.google.com/document/d/1pWfIToswRCDVpqTK1Bj5Uv6s-n7zpOaqgZHQjW3SNzU/edit?usp=sharing>");
        }
    }
}