/*SocketGuildUser usr = Context.Guild.GetUser(ID);
 */

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
    public class RacerCreation : ModuleBase<SocketCommandContext>
    {
        [Command("createracer")]
        public async Task NewracerAsync(string name, string faction)
        {
            Classes.racer racer = racer.get_racer(Context.Message.Author.Id);
            if(racer != null) {
                await ReplyAsync("You already have a racer. Please use `ta!deleteracer` to remove your old one first");
                return;
            }
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            if (name == null) {
                name = usr.Nickname != null ? usr.Nickname : usr.Username;
            }
            if(usr.Roles.FirstOrDefault(e=>e.Name == "Racer") == null) 
            { 
                await ReplyAsync(name + ", Please contact a moderator if you should be a racer");
            } 
            else 
            {
                racer = new racer
                {
                    name = name,
                    faction = faction
                };

                racer.player_discord_id = Context.Message.Author.Id;

                racer.insert_racer(racer);

                await ReplyAsync(name + ", You've got a racer!");
            }

        }

        [Command("showhand")]
        public async Task DisplayRacerHandAsync() {
            Classes.racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
            } else {
                List<string> str2 = new List<string>();
                str2.Add("**Your Current Hand**");
                if (racer.cards.Count == 0) { 
                    await ReplyAsync("Hold up, you don't have any cards. The game must not have started yet.");
                } else {
                    for(int i = 0; i < racer.cards.Count; i++) {
                        str2.Add("#" + (i+1) + ": " + racer.cards[i].ToString());
                    }
                    str2.Add("-- -- -- -- --");
                    str2.Add("**Current Hazards** - If any Hazard is applied for 3 turns, you will explode.");
                    if (racer.hazards.Count == 0) str2.Add("None");
                    int j = 0;
                    foreach (pair p in racer.hazards) {
                        str2.Add("#" + ++j + ": " + p.item1.title +" has been applied for " + p.item2 + " turns. " + id_to_condition[p.item1.ID]);
                    }
                    string output = String.Join(System.Environment.NewLine, str2);
                    await Context.User.SendMessageAsync(output);
                }
            }
        }

        private Dictionary<int, string> id_to_condition = new Dictionary<int, string> {
            {5,"You cannot move until you play a Dodge card."},
            {6, "You cannot move until you play a Dodge card."},
            {8, "You cannot move until you play a Tech Savvy card."},
            {9, "Can be removed by a Tech Savvy card. If you end your turn with both Sabotage and another Hazard, you explode."},
            {10, "Can be removed by a Cyber Healthcare card."},
            {11, "You cannot play Movement cards higher than 2. Can be removed by a Cyber Healthcare card."},
            {16, "You can not move this turn. Does not need a remedy to clear."}
        };

        [Command("deleteracer")]
        public async Task DeleteRacerAsync(params string[] args)
        {
            Classes.racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
            } else {

                Classes.racer.delete_racer(racer);
                await ReplyAsync("Racer Deleted.");
            }
        }


        [Command("listracers")]
        public async Task ListRacersAsync()
        {
            List<Classes.racer> racers = racer.get_racer();
            string s = "Racers for the Grand Prix!" + System.Environment.NewLine + "```" + System.Environment.NewLine;
            foreach(Classes.racer r in racers) {
                s += "ID: #" + r.ID + " | " + r.name + System.Environment.NewLine;
            }
            s += "```";
            await ReplyAsync(s);
        }
    }
}