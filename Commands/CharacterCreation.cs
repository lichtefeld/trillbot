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
    public class CharacterCreation : ModuleBase<SocketCommandContext>
    {
        [Command("registeraccount")]
        public async Task NewcharacterAsync()
        {
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var c = Character.get_character(Context.Message.Author.Id,Context.Guild.Id);
            if (c != null) { 
                await ReplyAsync("You already have an account!");
                return;
            }

            var name = usr.Nickname != null ? usr.Nickname : usr.Username;
            Server sr = Server.get_Server(Context.Guild.Id);
            int balance = 1000000;
            if (sr != null) {
                balance = sr.startingBalance;
            }
            c = new Character
            {
                name = name,
                balance = balance
            };

            c.player_discord_id = Context.Message.Author.Id;
            c.player_server_id = Context.Guild.Id;

            Character.insert_character(c);

            await ReplyAsync(name + ", you have created an account.");
            await Context.User.SendMessageAsync("Please note. This discord bot enables fictional gambling for the intent of RP creation. No real money is invested into this casino or gambling experience. Results here are not indicative of actual outcomes from a casino. If you have a gambling addiction, please do not continue to play and seek help from professionals.");

        }

        [Command("deleteaccount")]
        public async Task DeletecharacterAsync()
        {
            var c = Character.get_character(Context.Message.Author.Id,Context.Guild.Id);

            if(c == null) {
                await ReplyAsync("No character found for you");
            } else {

                Classes.Character.delete_character(c);
                await ReplyAsync("Account Deleted.");
            }
        }

        [Command("listaccounts")]
        public async Task ListcharactersAsync()
        {
            var characters = Character.get_character();

            await Context.User.SendMessageAsync("**Accounts**"+System.Environment.NewLine+string.Join(System.Environment.NewLine,characters.Where(e=>e.player_server_id==Context.Guild.Id).Select(e=>e.name).ToList()));
        }

        [Command("balance")]
        public async Task DisplayBalanceAsync() {
            //Display bets to the User in a DM?
            var character = Character.get_character(Context.Message.Author.Id,Context.Guild.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            await Context.User.SendMessageAsync("You have a current balance of " + character.balance + " imperial credits.");
        }

        [Command("bal")]
        public async Task displayBalAsync() {
            var character = Character.get_character(Context.Message.Author.Id,Context.Guild.Id);
            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            await ReplyAsync(Context.User.Mention + ", you have a current balance of " + character.balance + " imperial credits.");
        }

        [Command("bal")]
        public async Task displayBalAsync(SocketUser User) {
            var character = Character.get_character(User.Id,Context.Guild.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found.");
                return;
            }

            await ReplyAsync(character.name + " has a current balance of " + character.balance + " imperial credits.");
        }

        [Command("give")]
        public async Task giveMoneyAsync(SocketUser user, int amount) {
            var userChar = Character.get_character(Context.User.Id,Context.Guild.Id);
            if (userChar == null) {
                await ReplyAsync(Context.User.Mention + ", Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }
            var Char = Character.get_character(user.Id,Context.Guild.Id);
            if (Char == null) {
                await ReplyAsync(user + " does not have an account." );
                return;
            }

            if(userChar.balance < amount) {
                await ReplyAsync(Context.User.Mention + ", you don't have that much money");
                return;
            }

            Char.balance += amount;
            userChar.balance -= amount;

            await ReplyAsync(Context.User.Mention + ", you have transfered " + amount + ", to " + Char.name);

            Character.update_character(Char);
            Character.update_character(userChar);
        }

        [Command("add")]
        public async Task addMoneyAsync(SocketUser user, int amount) {
            var userChar = Character.get_character(user.Id,Context.Guild.Id);
            if (userChar == null) {
                await ReplyAsync(Context.User.Mention + ", Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", a server object doesn't exist for this server. Please create one.");
                return;
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }

            userChar.balance += amount;

            await ReplyAsync(Context.User.Mention + ", you have given " + amount + ", to " + userChar.name);

            Character.update_character(userChar);
        }

        [Command("sub")]
        public async Task subMoneyAsync(SocketUser user, int amount) {
            var userChar = Character.get_character(user.Id,Context.Guild.Id);
            if (userChar == null) {
                await ReplyAsync(Context.User.Mention + ", Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", a server object doesn't exist for this server. Please create one.");
                return;
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }

            userChar.balance -= amount;

            await ReplyAsync(Context.User.Mention + ", you have removed " + amount + ", to " + userChar.name);

            Character.update_character(userChar);
        }

    }
}