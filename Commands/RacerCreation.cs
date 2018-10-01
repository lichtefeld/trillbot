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
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            if (name == null) {
                name = usr.Nickname != null ? usr.Nickname : usr.Username;
            }
            if(usr.Roles.FirstOrDefault(e=>e.Name == "racer") == null) 
            { 
                await ReplyAsync(name + ", Please contact a moderator if you should be a racer");
            } 
            else 
            {
                Classes.racer racer = new racer
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
                string output = "";
                if (racer.cards == null) { 
                    await ReplyAsync("Hold up, you don't have any cards. The game must not have started yet.");
                } else {
                    foreach(Card c in racer.cards) {
                        output = c.ToString() + System.Environment.NewLine;
                    }
                    await Context.User.SendMessageAsync(output);
                }
            }
        }

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