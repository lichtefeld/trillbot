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

        //public static List<racer> allRacers = new List<racer>();

        [Command("createracer")]
        public async Task NewracerAsync(string name = null, string faction = "The Trilliant Ring", int ID = -1)
        {
            var r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);
            if(r != null) {
                await ReplyAsync("You already have a racer. Please use `ta!deleteracer` to remove your old one first");
                return;
            }
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            if (name == null) {
                name = usr.Nickname != null ? usr.Nickname : usr.Username;
            }
            if(ID == -1) {
                var a = Ability.get_ability(Program.rand.Next(25));
                r = new racer
                {
                    name = name,
                    faction = faction,
                    ability = a
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
            r.server_discord_id = Context.Guild.Id;
            //allRacers.Add(r);
            racer.insert_racer(r);

            await ReplyAsync(name + ", You've got a racer!");
        }

        [Command("showracer")]
        public async Task showRacerAsync(int i = -1) {
            racer r = new racer();
            if (i < 0) r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);
            else r = racer.get_racer(i);

            if ( r == null ) {
                await ReplyAsync(Context.User.Mention + ", you don't have a current racer or this racer doesn't exist in the database.");
                return;
            }

            var embed = new EmbedBuilder();

            embed.Title = "Grand Prix Racer: " + r.name;
            embed.AddField("Sponsor",r.faction, true);
            embed.AddField("Ability: " + r.ability.Title, r.ability.Description, true);
            embed.AddField("ID",r.ID.ToString(),true);
            if( i < 0 ) {
                embed.AddField("Player",Context.User.Mention,true);
            } else {
                var usr = Context.Guild.GetUser(r.player_discord_id);
                embed.AddField("Player",usr.Mention,true);
            }
            await Context.Channel.SendMessageAsync("", false, embed.Build(), null);
        }

        [Command("updateability")]
        public async Task UpdateAbilityAsync(int ID) {
            var r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);//racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {
                if (r.inGame) {
                    await ReplyAsync("You can't modify your racer while racing!");
                    return;
                }
                var a = Ability.get_ability(--ID);
                if(a == null) {
                    await ReplyAsync(Context.User.Mention + ", you didn't give a valid ID.");
                    return;
                }
                r.ability = a;
                racer.replace_racer(r);
                await ReplyAsync(Context.User.Mention + ", Ability changed to " + a.Title);
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
                if (count > 1950) {
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

        [Command("showcards")]
        public async Task showCardsAsync() {
            var cards = Card.get_card();
            var str = new List<string>();
            var count = 13;
            str.Add("**Card List**");
            for(int i = 0; i < cards.Count; i++) {
                var s = "**" + cards[i].title + "** - " + cards[i].description;
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
            var r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);//racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {

                Classes.racer.delete_racer(r);
                await ReplyAsync("Racer Deleted.");
            }
        }

        [Command("resetracer")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetOneRacer(int i) {
            var r = racer.get_racer(i);
            if (r == null) {
                await ReplyAsync("No racer with that ID");
                return;
            }
            r.reset();
            racer.replace_racer(r);
            await ReplyAsync(r.nameID() + " has been reset");
        }

        [Command("resetracers")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetAllRacers() {
            var rcs = racer.get_racer();
            rcs.ForEach(e=>{
                e.inGame = false;
                racer.update_racer(e);
                });           
            await ReplyAsync("All Racers Reset");
        }

        [Command("listracers")]
        public async Task ListRacersAsync() //Need to make this DM & account for more than 2k characters. Using a list to build output strings.
        {
            var s = new List<string>();
            s.Add("Racers for the Grand Prix!");
            s.Add("```" );
            var rcrs = racer.get_racer();
            foreach(Classes.racer r in rcrs) {
                s.Add("ID: #" + r.ID + " | " + r.name);
            }
            s.Add("```");
            await ReplyAsync(String.Join(System.Environment.NewLine,s));
            return;
        }
    }
}