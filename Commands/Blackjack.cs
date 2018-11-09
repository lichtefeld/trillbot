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
            if(b < bj.Value.minbet || b > bj.Value.maxbet) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", sorry this table has a minimum bet of " + bj.Value.minbet + " and a maximum bet of " + bj.Value.maxbet + ". You must bet between those values.");
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
            bj.Value.runGame(Context);
        }

        [Command("blackjack")]
        public async Task explainBlackjackAsync() {
            var str = new List<string>();
            str.Add("Blackjack is a casino banked game, meaning that players compete against the house rather than each other. The objective is to get a hand total of closer to 21 than the dealer without going over 21 (busting).");
            str.Add("At the start of a Blackjack game, the players and the dealer receive two cards each. The players' cards are normally dealt face up, while the dealer has one face down (called the hole card) and one face up. The best possible Blackjack hand is an opening deal of an ace with any ten-point card.");
            str.Add("When playing Blackjack the numeral cards 2 to 10 have their face values, Jacks, Queens and Kings are valued at 10, and Aces can have a value of either 1 or 11. The Ace is always valued at 11 unless that would result in the hand going over 21, in which case it is valued as 1.");
            str.Add("A starting hand of a 10 valued card and an Ace is called a Blackjack or natural and beats all hands other than another Blackjack. If both the player and dealer have Blackjack, the result is a push (tie): neither the player nor the bank wins and the bet is returned to the player.");
            str.Add("If the dealer is showing an ACE they will offer a bet called \"insurance\" which pays out if the dealer has blackjack.");
            str.Add("**Standard Actions**");
            str.Add("__Stand__ - Accept your hand as is");
            str.Add("__Hit__ - Take Another Card to your hand");
            str.Add("__Double__ - Double your bet, and take one more card, then automatically Stand");
            str.Add("__Split__ - If you have two matching value cards you can split your hand into an additional one. You can have upto 4 hands");
            str.Add("__Surrender__ - If taken as the first option on your turn you receive 50% of your bet back");
            str.Add("**Payouts**");
            str.Add("__Push (Tie)__ - You get your bet back");
            str.Add("__Win__ - Pays 1:1, bet 10 get back 20 (10 + 10)");
            str.Add("__Blackjack__ - Pays 3:2, bet 10 get back 25 (10 + 15)");
            str.Add("__Insurance__ - Pays 2:1, bet 10, insure 5, get back 15 (5 + 10)");
            str.Add("More Information: <https://www.pagat.com/banking/blackjack.html>");
            await Context.User.SendMessageAsync(String.Join(System.Environment.NewLine,str));
        }
    }
}
