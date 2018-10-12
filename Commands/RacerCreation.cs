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
            var r = racer.get_racer(Context.Message.Author.Id);
            if(r != null) {
                await ReplyAsync("You already have a racer. Please use `ta!deleteracer` to remove your old one first");
                return;
            }
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
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
                    r = new racer
                    {
                        name = name,
                        faction = faction
                    };
                } else {
                    var a = Ability.get_ability(--ID);
                    r = new racer
                    {
                        name = name,
                        faction = faction,
                        ability = a
                    };
                }

                r.player_discord_id = Context.Message.Author.Id;

                racer.insert_racer(r);

                await ReplyAsync(name + ", You've got a racer!");
            }

        }

        [Command("showracer")]
        public async Task showRacerAsync() {
            var r = racer.get_racer(Context.Message.Author.Id);
            if(r == null) {
                await ReplyAsync("You don't have a racer.");
            } else {
                await ReplyAsync("",false,helpers.ObjToEmbed(r,"name"),null);
            }
        }

        [Command("updateability")]
        public async Task UpdateAbilityAsync(int ID) {
            var r = racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {
                if (r.inGame) {
                    await ReplyAsync("You can't modify your racer while racing!");
                    return;
                }
                var a = Ability.get_ability(--ID);
                r.ability = a;
                await ReplyAsync("Ability changed to " + a.Title);
            }
        }

        [Command("showabilities")]
        public async Task DisplayAbilitiesAsync() {
            var abilities = Ability.get_ability();
            var str = new List<string>();
            var count = 21;
            str.Add("**Special Abilities**");
            for(int i = 0; i < abilities.Count; i++) {
                var active = "Passive";
                if (abilities[i].Active){
                    active = "Active";
                }
                var s = "**#" + (i+1) + ":** " + abilities[i].Title + " (" + active + ") - *" +abilities[i].Description + "*";
                count += s.Length;
                if (count > 2000) {
                    var temp_output_string = String.Join(System.Environment.NewLine,str);
                    await Context.User.SendMessageAsync(temp_output_string);
                    count = s.Length;
                    str = new List<string>();
                }
                str.Add(s);
            }
            var output_string = String.Join(System.Environment.NewLine,str);
            await Context.User.SendMessageAsync(output_string);
        }

        [Command("deleteracer")]
        public async Task DeleteRacerAsync()
        {
            var r = racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {

                Classes.racer.delete_racer(r);
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
        public async Task ListRacersAsync() //Need to make this DM & account for more than 2k characters. Using a list to build output strings.
        {
            var racers = racer.get_racer();
            var s = new List<string>();
            s.Add("Racers for the Grand Prix!");
            s.Add("```" );
            foreach(Classes.racer r in racers) {
                s.Add("ID: #" + r.ID + " | " + r.name);
            }
            s.Add("```");
            await ReplyAsync(String.Join(System.Environment.NewLine,s));
            return;
        }
    }
}