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

        public static List<racer> allRacers = new List<racer>();
        private static Dictionary<int, string> racer_to_Image = new Dictionary<int, string>() {
            {1, "https://vignette.wikia.nocookie.net/far-verona/images/d/d1/VladHumble.png/revision/latest?cb=20181029143832"},
            {2, "https://vignette.wikia.nocookie.net/far-verona/images/1/11/Franciszek.png/revision/latest?cb=20181025131015"},
            {3, "https://vignette.wikia.nocookie.net/far-verona/images/1/12/StrixVulpine.png/revision/latest?cb=20181025131654"},
            {4, "https://vignette.wikia.nocookie.net/far-verona/images/9/90/PhasTee.png/revision/latest?cb=20181027151400"},
            {5, ""}, //Anthony
            {6, "https://vignette.wikia.nocookie.net/far-verona/images/2/26/DulaImay.png/revision/latest?cb=20181025125348"},
            {7,"https://vignette.wikia.nocookie.net/far-verona/images/7/73/CocoCobra.png/revision/latest?cb=20181025133404"},
            {8, ""}, //Crux Kresler
            {9, "https://vignette.wikia.nocookie.net/far-verona/images/8/81/RutileVenus.png/revision/latest?cb=20181025132952"},
            {10, "https://vignette.wikia.nocookie.net/far-verona/images/7/7c/SJacobson.png/revision/latest?cb=20181025125606"},
            {11, "https://vignette.wikia.nocookie.net/far-verona/images/6/61/DeciusTulliusCrispus.png/revision/latest?cb=20181025131248"},
            {12, "https://vignette.wikia.nocookie.net/far-verona/images/0/03/RacerIX.png/revision/latest?cb=20181025133954"},
            {13, "https://vignette.wikia.nocookie.net/far-verona/images/d/d5/DeciusCato.png/revision/latest?cb=20181025131900"},
            {14, "https://vignette.wikia.nocookie.net/far-verona/images/c/c5/LionoPanthra.png/revision/latest?cb=20181025132105"},
            {15, "https://vignette.wikia.nocookie.net/far-verona/images/1/17/JaxtonBenson.png/revision/latest?cb=20181029144044"},
            {16, "https://vignette.wikia.nocookie.net/far-verona/images/0/01/Amber.png/revision/latest?cb=20181025134236"},
            {17, "https://vignette.wikia.nocookie.net/far-verona/images/3/38/TheMoose.png/revision/latest?cb=20181025133616"},
            {18, "https://vignette.wikia.nocookie.net/far-verona/images/4/43/CruxPanda.png/revision/latest?cb=20181025131452"},
            {19, "https://vignette.wikia.nocookie.net/far-verona/images/6/6e/TyrenePrayla.png/revision/latest?cb=20181025132657"},
            {20, "https://vignette.wikia.nocookie.net/far-verona/images/a/a4/RhodesBiggles.png/revision/latest?cb=20181025132247"},
            {21, "https://vignette.wikia.nocookie.net/far-verona/images/6/63/MongrelJimTimo.png/revision/latest?cb=20181025195613"},
            {22, "https://vignette.wikia.nocookie.net/far-verona/images/a/ac/RichieSteel.png/revision/latest?cb=20181025132505"},
            {23, ""}, //Vela
            {24, "https://vignette.wikia.nocookie.net/far-verona/images/6/63/GuillaumeValls.png/revision/latest?cb=20181025130707"},
            {25, "https://vignette.wikia.nocookie.net/far-verona/images/5/54/Nikita.png/revision/latest?cb=20181025130043"}
        };

        private static Dictionary<int, string> racer_to_Description = new Dictionary<int, string>() {
            {1, "The ninth child of the infamous and un-killable mercenary Dimi of the Deathless, Vlad is pumped full of gene mods and ego. He has an unhealthy confidence in his virility and physique and is not ashamed to let everyone know it; \"humble\" he certainly is not. We all know that muscles, boasts and gene mods don't win races but as a racer in this year Prix, Dimi's son does have one thing to brag about: he is an extraordinary precog trained on Hroa. Most years the Prix sees large numbers of precogs enter the lists as their reaction times and predictive driving techniques prove incredibly advantageous when traveling in the shrapnel filled Lightway at ludicrous speeds. This year however, the only precog openly advertising themselves is Vlad Humble himself, it may be that the race this year is stacked with precogs too clever to out themselves publicly, but who can say. Vlad claims to have been told his whole life that he is absolutely perfect in every way: physically, mentally and emotionally; perhaps he has been treated with favoritism due to his father's deadly fame but maybe Vlad truly has nothing to be humble about. Is Vlad practically perfect in every way? Will Dimi find you and kill you if you say otherwise? Get your betting money ready and find out!"},
            {2,""}
        };

        [Command("createracer")]
        public async Task NewracerAsync(string name, string faction, int ID = -1)
        {
            var r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);
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
                allRacers.Add(r);
                racer.insert_racer(r);

                await ReplyAsync(name + ", You've got a racer!");
            }

        }

        [Command("showracer")]
        public async Task showRacerAsync(int i = -1) {
            racer r = new racer();
            if (i < 0) r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);
            else r = allRacers[i];
            var embed = new EmbedBuilder();

            embed.Title = "Grand Prix Racer: " + r.name;
            if (r.ID <= 25 && r.ID != 0) {
                embed.WithThumbnailUrl(racer_to_Image[r.ID]);
                embed.WithDescription(racer_to_Description[r.ID]);
            }
            embed.AddField("Sponsor",r.faction,true);
            embed.AddField("Ability: " + r.ability.Title, r.ability.Description, true);
            embed.Build();
            await Context.Channel.SendMessageAsync("", false, embed, null);
        }

        [Command("updateability")]
        public async Task UpdateAbilityAsync(int ID) {
            var r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {
                if (r.inGame) {
                    await ReplyAsync("You can't modify your racer while racing!");
                    return;
                }
                var a = Ability.get_ability(--ID);
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
            var r = allRacers.FirstOrDefault(e=> e.player_discord_id == Context.Message.Author.Id);//racer.get_racer(Context.Message.Author.Id);

            if(r == null) {
                await ReplyAsync("No racer found for you");
            } else {

                Classes.racer.delete_racer(r);
                allRacers.Remove(r);
                await ReplyAsync("Racer Deleted.");
            }
        }

        [Command("resetracer")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task resetOneRacer(int i) {
            var r = allRacers.FirstOrDefault(e=> e.ID == i);
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
            allRacers.ForEach(e=> {
                e.reset();
            });
            helpers.UpdateRacersDatabase();            
            await ReplyAsync("All Racers Reset");
        }

        [Command("listracers")]
        public async Task ListRacersAsync() //Need to make this DM & account for more than 2k characters. Using a list to build output strings.
        {
            var s = new List<string>();
            s.Add("Racers for the Grand Prix!");
            s.Add("```" );
            foreach(Classes.racer r in allRacers) {
                s.Add("ID: #" + r.ID + " | " + r.name);
            }
            s.Add("```");
            await ReplyAsync(String.Join(System.Environment.NewLine,s));
            return;
        }
    }
}