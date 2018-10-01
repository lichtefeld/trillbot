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
using RestSharp;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class Bet : ModuleBase<SocketCommandContext>
    {
        [Command("bet")]
        public async Task BetAsync(int ID, int amount)
        {
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            Character character = Classes.Character.get_character(Context.Message.Author.Id);
            racer racer = racer.get_racer(ID);
            string name = usr.Nickname != null ? usr.Nickname : usr.Username;

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `tb!registeraccount`");
                return;
            }

            if(racer == null) {
                await ReplyAsync("The racer you selected doesn't exist.");
                return;
            }
            
            if (amount <= 0) {
                await ReplyAsync("You can't make a negative bet!");
                return;
            }

            trillbot.Classes.Bet b = new trillbot.Classes.Bet(racer.name,amount);

            character.bets.Add(b);
            character.balance -= amount;
            Character.update_character(character);
            await ReplyAsync(name + ", you have placed the following bet: " + System.Environment.NewLine + b.ToString());

        }

        [Command("displaybets")]
        public async Task DisplaybetsAsync()
        {
            //Display bets to the User in a DM?
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            Character character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `tb!registeraccount`");
                return;
            }

            string output = "**" +character.name + " Bets** ```" + System.Environment.NewLine;

            foreach (trillbot.Classes.Bet bet in character.bets) {
                output+= bet.ToString() + "\n";
            }

            output += "```";

            await Context.User.SendMessageAsync(output);
        }

        [Command("displaybalance")]
        public async Task DisplayBalanceAsync() {
            //Display bets to the User in a DM?
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            Character character = Character.get_character(Context.Message.Author.Id);

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
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            Character character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `tb!registeraccount`");
                return;
            }

            foreach (trillbot.Classes.Bet bet in character.bets) {
                if (bet.Id == ID) {
                    character.balance += bet.Amount;
                    character.bets.Remove(bet);
                    await ReplyAsync("Bet with ID: " + ID + "has been cancled.");
                    return;
                }
            }

            await ReplyAsync("Bet not found. You can see the list of bets you've made with `tb!displaybets`");
            return;
        }

        
    }
}