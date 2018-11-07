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

        public static readonly List<Tuple<string,int,int,double,string>> emote_to_ID = new List<Tuple<string,int,int,double,string>>() {
            //Emote String, ID, Winning, Death, Name
            {new Tuple<string,int, int, double, string>(":vlad:",1,28,45.0,"Vlad \"Humble\" Antonovich")},
            {new Tuple<string,int, int, double, string>(":franz:",2,44,2.0,"Franciszek the Knight-Errant")},
            {new Tuple<string,int, int, double, string>(":sportivox:",3,4,31.0,"Sportivox Eridanus Strix Vulpine")},
            {new Tuple<string,int, int, double, string>(":phastee:",4,18,50.0,"Mister Phas'Tee")},
            {new Tuple<string,int, int, double, string>(":anthony:",5,25,40.0,"Anthony Gonzales")},
            {new Tuple<string,int, int, double, string>(":dula:",6,42,21.0,"Dula Imay")},
            {new Tuple<string,int, int, double, string>(":coco:",7,9,25.0,"Coco Cobra")},
            {new Tuple<string,int, int, double, string>(":tiyamike:",8,29,8.0,"Kreigsherr Crux Kesler Tiyamike")},
            {new Tuple<string,int, int, double, string>(":rutile:",9,29,15.0,"Rutile Venus")},
            {new Tuple<string,int, int, double, string>(":jacobson:",10,39,25.0,"S. Jacobson")},
            {new Tuple<string,int, int, double, string>(":decius:",11,3,27.0,"Decius Tullius Crispus")},
            {new Tuple<string,int, int, double, string>(":racerIX:",12,37,5.0,"Racer IX")},
            {new Tuple<string,int, int, double, string>(":cato:",13,45,15.0,"Princeps Fornax Decius Cato")},
            {new Tuple<string,int, int, double, string>(":liono:",14,48,7.0,"Xeper Lyra Liono Panthra")},
            {new Tuple<string,int, int, double, string>(":jaxton:",15,16,10.0,"Jaxton Benson")},
            {new Tuple<string,int, int, double, string>(":amber:",16,32,31.0,"Amber")},
            {new Tuple<string,int, int, double, string>(":moose:",17,11,5.0,"The Moose")},
            {new Tuple<string,int, int, double, string>(":panda:",18,25,35.0,"Crux Panda")},
            {new Tuple<string,int, int, double, string>(":prayla:",19,16,44.0,"Prayla Tyrene, MO, Cybernetics")},
            {new Tuple<string,int, int, double, string>(":biggles:",20,10,5.0,"Echo Pyxis Rhodes Biggles")},
            {new Tuple<string,int, int, double, string>(":jim:",21,14,21.0,"\"Mongrel\" Jim Timo")},
            {new Tuple<string,int, int, double, string>(":steel:",22,45,30.0,"Richie Steel")},
            {new Tuple<string,int, int, double, string>(":fuschia:",23,42,1.5,"Shamshir Vela Fadi “Fuschia” Cadis")},
            {new Tuple<string,int, int, double, string>(":valls:",24,8,46.0,"PFC Guillaume \"Useless\" Valls")},
            {new Tuple<string,int, int, double, string>(":nikita:",25,40,7.0,"Nikita")}
        };

        [Command("bet")]
        public async Task BetAsync(string type, string s, int amount)
        {
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var name = usr.Nickname != null ? usr.Nickname : usr.Username;
            var character = Classes.Character.get_character(Context.Message.Author.Id);
            var emoteName = helpers.parseEmote(s);
            //await ReplyAsync(emoteName);
            var betInfo = emote_to_ID.FirstOrDefault(e=> e.Item1 == emoteName);

            if(!acceptBets) {
                await ReplyAsync("Sorry, we are no longer accepting bets!");
                return;
            }

            if(betInfo == null) {
                await ReplyAsync("Please submit input of a racer using an emote");
                return;
            }

            var r = racer.get_racer(betInfo.Item2);

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
            type = type.ToLower();
            if (type != "win" && type != "death") {
                await ReplyAsync("Non-valid bet type. Only `win` and `death` accepted");
                return;
            }
            if(character.balance-amount<0) {
                await ReplyAsync("You can't go into a negative balance");
                return;
            }
            var emotesList = Context.Guild.Emotes.ToList();
            var em = emotesList.FirstOrDefault(e=> e.Name == betInfo.Item1.Substring(1,betInfo.Item1.Length-2));
            var b = new trillbot.Classes.Bet(character.bets.Count,betInfo.Item5,amount,type,em.Name);

            character.bets.Add(b);
            character.balance -= amount;
            Character.update_character(character);

            if(type == "death") {
                await ReplyAsync("Thank you " + usr.Mention + "! Your donation will support Hong Lu's Orphans!" + System.Environment.NewLine + "you have placed the following bet: " + System.Environment.NewLine + b.display(Context.Guild));
            } else {
                await ReplyAsync(usr.Mention + ", you have placed the following bet: " + System.Environment.NewLine + b.display(Context.Guild));
            }
        }

        [Command("displaybets")]
        public async Task DisplaybetsAsync(IUser User = null)
        {
            //Display bets to the User in a DM?
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var character = Character.get_character(Context.Message.Author.Id);

            if (character == null)
            {
                await ReplyAsync("Account not found. Please create one before proceeding via `ta!registeraccount`");
                return;
            }
            
            await Context.User.SendMessageAsync(helpers.formatBets(character, Context.Guild));
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

        [Command("odds")]
        public async Task displayRacerOdds(bool channel = false) {
            var strings = new List<string>();
            var serverEmotes = Context.Guild.Emotes.ToList();
            strings.Add("ID) Name | Emote Title | Winning Bet");
            foreach(var k in emote_to_ID) {
                var emote = serverEmotes.FirstOrDefault(e=> e.Name == k.Item1.Substring(1,k.Item1.Length-2));
                strings.Add(k.Item2 +") "+k.Item5+" | " + emote + " `"+k.Item1+"` | 1 to "+k.Item3);
            }
            int count = 0;
            string output_string = "";
            foreach(string s in strings) {
                count += s.Length + 1;
                if (count >= 2000) {
                    if (channel) await ReplyAsync(output_string);
                    await Context.User.SendMessageAsync(output_string);
                    count = s.Length;
                    output_string = s + System.Environment.NewLine;
                } else {
                    output_string += s + System.Environment.NewLine;
                }
            }
            if (channel) await ReplyAsync(output_string);
            await Context.User.SendMessageAsync(output_string);
        }

        [Command("allbets")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task allBetsAsync() {
            var chars = Character.get_character();
            var strings = new List<string>();
            foreach(Character c in chars) {
                strings.Add(helpers.formatBets(c, Context.Guild));
            }
            int count = 0;
            string output_string = "";
            foreach(string s in strings) {
                count += s.Length + 1;
                if (count >= 2000) {
                    await ReplyAsync(output_string);
                    count = s.Length;
                    output_string = s + System.Environment.NewLine;
                } else {
                    output_string += s + System.Environment.NewLine;
                }
            }
            await ReplyAsync(output_string);
        }

        [Command("togglebets")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task toggleBetsAsync() {
            acceptBets = !acceptBets;
            await ReplyAsync("Betting acceptance toggled to: " + acceptBets);
        }

    }
}