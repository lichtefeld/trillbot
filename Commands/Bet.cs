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
    public class Bet : ModuleBase<SocketCommandContext>
    {
        [Command("bet")]
        public async Task BetAsync(int ID, int amount)
        {
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var character = Classes.Character.get_character(Context.Message.Author.Id);
            var r = racer.get_racer(ID);
            var name = usr.Nickname != null ? usr.Nickname : usr.Username;

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            if(r == null) {
                await ReplyAsync("The racer you selected doesn't exist.");
                return;
            }
            
            if (amount <= 0) {
                await ReplyAsync("You can't make a negative bet!");
                return;
            }

            var b = new trillbot.Classes.Bet(r.name,amount);

            character.bets.Add(b);
            character.balance -= amount;
            Character.update_character(character);
            await ReplyAsync(name + ", you have placed the following bet: " + System.Environment.NewLine + b.ToString());

        }

        [Command("displaybets")]
        public async Task DisplaybetsAsync()
        {
            //Display bets to the User in a DM?
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            var output = new List<string>();
            output.Add("**" +character.name + " Bets** ```");
            foreach (trillbot.Classes.Bet bet in character.bets) {
                output.Add(bet.ToString());
            }
            output.Add("```");
            var output_string = String.Join(System.Environment.NewLine,output);
            await Context.User.SendMessageAsync(output_string);
        }

        [Command("displaybalance")]
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

        [Command("cancelbet")]
        public async Task CancelbetAsync(int ID)
        {
            //Allow a user to cancel a bet
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }

            foreach (var bet in character.bets) {
                if (bet.Id == ID) {
                    character.balance += bet.Amount;
                    character.bets.Remove(bet);
                    await ReplyAsync("Bet with ID: " + ID + "has been cancled.");
                    return;
                }
            }

            await ReplyAsync("Bet not found. You can see the list of bets you've made with `ta!displaybets`");
            return;
        }

        
    }
}