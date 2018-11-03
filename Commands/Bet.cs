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
        public static Boolean acceptBets = true;

        private static readonly Dictionary<string,Tuple<int,int,double>> emote_to_ID = new Dictionary<string, Tuple<int,int,double>>() {
            {":vlad:",new Tuple<int, int, double>(1,28,45.0)},
            {":franz:",new Tuple<int, int, double>(2,44,2.0)},
            {":sportivox:",new Tuple<int, int, double>(3,4,31.0)},
            {":phastee:",new Tuple<int, int, double>(4,18,50.0)},
            {":anthony:",new Tuple<int, int, double>(5,25,40.0)},
            {":dula:",new Tuple<int, int, double>(6,42,21.0)},
            {":coco:",new Tuple<int, int, double>(7,9,25.0)},
            {":tiyamike:",new Tuple<int, int, double>(8,29,8.0)},
            {":rutile:",new Tuple<int, int, double>(9,29,15.0)},
            {":jacobson:",new Tuple<int, int, double>(10,39,25.0)},
            {":decius:",new Tuple<int, int, double>(11,3,27.0)},
            {":racerIX:",new Tuple<int, int, double>(12,37,5.0)},
            {":cato:",new Tuple<int, int, double>(13,45,15.0)},
            {":liono:",new Tuple<int, int, double>(14,48,7.0)},
            {":jaxton:",new Tuple<int, int, double>(15,16,10.0)},
            {":amber:",new Tuple<int, int, double>(16,32,31.0)},
            {":moose:",new Tuple<int, int, double>(17,11,5.0)},
            {":panda:",new Tuple<int, int, double>(18,25,35.0)},
            {":prayla:",new Tuple<int, int, double>(19,16,44.0)},
            {":biggles:",new Tuple<int, int, double>(20,10,5.0)},
            {":jim:",new Tuple<int, int, double>(21,14,21.0)},
            {":steel:",new Tuple<int, int, double>(22,45,30.0)},
            {":fuschia:",new Tuple<int, int, double>(23,42,1.5)},
            {":valls:",new Tuple<int, int, double>(24,8,46.0)},
            {":nikita:",new Tuple<int, int, double>(25,40,7.0)}
        };

        [Command("bet")]
        public async Task BetAsync(string s, int amount)
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
            await ReplyAsync(usr.Mention + ", you have placed the following bet: " + System.Environment.NewLine + b.ToString());

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
            
            await Context.User.SendMessageAsync(helpers.formatBets(character));
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
                    await ReplyAsync("Bet with ID: " + ID + " has been cancled.");
                    Character.update_character(character);
                    return;
                }
            }

            await ReplyAsync("Bet not found. You can see the list of bets you've made with `ta!displaybets`");
            return;
        }

        [Command("allbets")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task allBetsAsync() {
            var chars = Character.get_character();
            var strings = new List<string>();
            foreach(Character c in chars) {
                strings.Add(helpers.formatBets(c));
            }
            helpers.output(Context.Channel,strings);
        }
    }
}