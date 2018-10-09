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
using trillbot.Classes;

namespace trillbot.Commands
{
    public class RacerCreation : ModuleBase<SocketCommandContext>
    {
        [Command("createracer")]
        public async Task NewracerAsync(string name, string faction, int ID = -1)
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
                if(ID == -1) {
                    racer = new racer
                    {
                        name = name,
                        faction = faction
                    };
                } else {
                    Ability a = Ability.get_ability(--ID);
                    racer = new racer
                    {
                        name = name,
                        faction = faction,
                        ability = a
                    };
                }

                racer.player_discord_id = Context.Message.Author.Id;

                racer.insert_racer(racer);

                await ReplyAsync(name + ", You've got a racer!");
            }

        }

        [Command("showracer")]
        public async Task showRacerAsync() {
            Classes.racer racer = racer.get_racer(Context.Message.Author.Id);
            if(racer == null) {
                await ReplyAsync("You don't have a racer.");
            } else {
                await ReplyAsync("",false,helpers.ObjToEmbed(racer,"name"),null);
            }
        }

        [Command("updateabiliy")]
        public async Task UpdateAbilityAsync(int ID) {
            Classes.racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
            } else {
                if (racer.inGame) {
                    await ReplyAsync("You can't modify your racer while racing!");
                    return;
                }
                Ability a = Ability.get_ability(--ID);
                racer.ability = a;
                await ReplyAsync("Ability changed to " + a.Title);
            }
        }

        [Command("showabilities")]
        public async Task DisplayAbilitiesAsync() {
            List<Classes.Ability> abilities = Ability.get_ability();
            List<string> str = new List<string>();
            int count = 21;
            str.Add("**Special Abilities**");
            for(int i = 0; i < abilities.Count; i++) {
                string s = "**#" + (i+1) + ":** " + abilities[i].Title + " (" + abilities[i].Type + ") - *" +abilities[i].Description + "*";
                count += s.Length;
                if (count > 2000) {
                    string temp_output_string = String.Join(System.Environment.NewLine,str);
                    await Context.User.SendMessageAsync(temp_output_string);
                    count = s.Length;
                    str = new List<string>();
                }
                str.Add(s);
            }
            string output_string = String.Join(System.Environment.NewLine,str);
            await Context.User.SendMessageAsync(output_string);
        }

        [Command("showhand")]
        public async Task DisplayRacerHandAsync() {
            Classes.racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
            } else {
                await Context.User.SendMessageAsync(racer.currentStatus());
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

        [Command("resetracers")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetAllRacers() {
            var racers = racer.get_racer();
            racers.ForEach(e=> {
                e.reset();
                racer.replace_racer(e);
            });
            await ReplyAsync("All Racers Reset");
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