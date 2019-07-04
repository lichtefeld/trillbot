using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class RacerCreation : ModuleBase<SocketCommandContext>
    {

        [Command("createracer")]
        public async Task NewracerAsync(string name = null, string faction = "The Trilliant Ring", int ID = -1, int v = -1)
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
            Server sr = Server.get_Server(Context.Guild.Id);
            if (v == -1) {
                if (sr == null) v = 0;
                else v = sr.racingVersionDefault;
            }
            var vT = textVersion.get_textVersion(v);
            if(ID == -1) {
                var a = Ability.get_ability(vT.abilityStore,Program.rand.Next(25));
                r = new racer
                {
                    name = name,
                    faction = faction,
                    ability = a
                };
            } else {
                var a = Ability.get_ability(vT.abilityStore,--ID);
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

        [Command("setdesc")]
        public async Task addDescAsync(string desc) {
            racer r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);
            if ( r == null ) {
                await ReplyAsync(Context.User.Mention + ", you don't have a current racer or this racer doesn't exist in the database.");
                return;
            }
            r.descr = desc;
            await ReplyAsync(Context.User.Mention + ", you have added a description to your racer.");
            racer.update_racer(r);
        }

        [Command("setImg")]
        public async Task addImgAsync(string img) {
            racer r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);
            if ( r == null ) {
                await ReplyAsync(Context.User.Mention + ", you don't have a current racer or this racer doesn't exist in the database.");
                return;
            }
            r.img = img;
            await ReplyAsync(Context.User.Mention + ", you have added an image to your racer.");
            racer.update_racer(r);
        }

        [Command("racer")]
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
            embed.WithDescription(r.descr);
            embed.WithThumbnailUrl(r.img);
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
        public async Task UpdateAbilityAsync(int ID, int v = -1) {
            var r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {
                if (r.inGame) {
                    await ReplyAsync("You can't modify your racer while racing!");
                    return;
                }
                textVersion tV;
                if ( v < 0 ) {
                    var s = Server.get_Server(Context.Guild.Id);
                    if ( s == null ) tV = textVersion.get_textVersion(0);
                    else tV = textVersion.get_textVersion(s.racingVersionDefault);
                } else {
                    tV = textVersion.get_textVersion(v);
                }
                var a = Ability.get_ability(tV.abilityStore,--ID);
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
        public async Task DisplayAbilitiesAsync(int v = -1) {
            Server sr = Server.get_Server(Context.Guild.Id);
            if (sr == null) v = 0;
            textVersion vT;
            if (v == -1) {
                vT = textVersion.get_textVersion(sr.racingVersionDefault);
                if (vT == null) vT = textVersion.get_textVersion(0);
            } else {
                vT = textVersion.get_textVersion(v);
            }
            var abilities = Ability.get_ability(vT.abilityStore);
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
        public async Task showCardsAsync(int v = -1) {
            Server sr = Server.get_Server(Context.Guild.Id);
            if (sr == null) v = 0;
            textVersion vT;
            if (v == -1) {
                vT = textVersion.get_textVersion(sr.racingVersionDefault);
                if (vT == null) vT = textVersion.get_textVersion(0);
            } else {
                vT = textVersion.get_textVersion(v);
            }
            var cards = Card.get_card(vT.cardStore);
            var str = new List<string>();
            str.Add("**Card List**");
            for(int i = 0; i < cards.Count; i++) {
                str.Add("**" + cards[i].title + "** - " + cards[i].description);
            }
            helpers.output(Context.User,str);
        }

        [Command("deleteracer")]
        public async Task DeleteRacerAsync()
        {
            var r = racer.get_racer(Context.Message.Author.Id, Context.Guild.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {

                Classes.racer.delete_racer(r);
                await ReplyAsync("Racer Deleted.");
            }
        }

        [Command("resetracer")]
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetOneRacer(int i) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't authorized on this server.");
                return;
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
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
        //[RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetAllRacers() {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't authorized on this server.");
                return;
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            var rcs = racer.get_racer();
            rcs.ForEach(e=>{
                if ( e.server_discord_id == Context.Guild.Id ) {
                    e.inGame = false;
                    racer.update_racer(e);
                }
            });
            await ReplyAsync("All Racers on this Server Reset");
        }

        [Command("listracers")]
        public async Task ListRacersAsync() //Need to make this DM & account for more than 2k characters. Using a list to build output strings.
        {
            var s = new List<string>();
            s.Add("Racers!");
            s.Add("```" );
            var rcrs = racer.get_racer();
            foreach(Classes.racer r in rcrs) {
                if (r.server_discord_id == Context.Guild.Id) s.Add("ID: #" + r.ID + " | " + r.name);
            }
            s.Add("```");
            await ReplyAsync(String.Join(System.Environment.NewLine,s));
            return;
        }
    }
}