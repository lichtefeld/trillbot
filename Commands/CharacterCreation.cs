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
            var c = Character.get_character(Context.Message.Author.Id);
            if (c != null) { 
                await ReplyAsync("You already have an account!");
                return;
            }

            var name = usr.Nickname != null ? usr.Nickname : usr.Username;
            
            c = new Character
            {
                name = name
            };

            c.player_discord_id = Context.Message.Author.Id;

            Character.insert_character(c);

            await ReplyAsync(name + ", you have created an account. You can now use ta!bet <racer> <amount>");

        }

        [Command("deleteaccount")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeletecharacterAsync()
        {
            var c = Character.get_character(Context.Message.Author.Id);

            if(c == null) {
                await ReplyAsync("No character found for you");
            } else {

                Classes.Character.delete_character(c);
                await ReplyAsync("Account Deleted.");
            }
        }

        [Command("listaccounts")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListcharactersAsync()
        {
            var characters = Character.get_character();

            await ReplyAsync("**Racers for the Grand Prix**"+System.Environment.NewLine+string.Join(System.Environment.NewLine,characters.Select(e=>e.name).ToList()));
        }

        [Command("balance")]
        public async Task DisplayBalanceAsync() {
            //Display bets to the User in a DM?
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            await Context.User.SendMessageAsync("You have a current balance of " + character.balance + " imperial credits.");
        }

        [Command("bal")]
        public async Task displayBalAsync() {
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            await ReplyAsync(Context.User.Mention + ", you have a current balance of " + character.balance + " imperial credits.");
        }

        [Command("give")]
        public async Task giveMoneyAsync(SocketUser user, int amount) {
            var userChar = Character.get_character(Context.User.Id);
            if (userChar == null) {
                await ReplyAsync(Context.User.Mention + ", Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }
            var Char = Character.get_character(user.Id);
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
    }
}
