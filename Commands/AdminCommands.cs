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
using RestSharp;

namespace trillbot.Commands
{
    public class leaderboard
    {
        public string name {get;set;}
        public long credits {get;set;}
    }
    public class leaderboard_stats
    {
        public List<leaderboard> leaderboard { get; set; }
        public string api_key { get; set; }
    }
    public class AdminCommands : ModuleBase<SocketCommandContext>
    {

        [Command("add")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task addFundsAsync(SocketUser user, int i)
        {
            var c = Character.get_character(user.Id);
            if (c == null)
            {
                await ReplyAsync("This user doesn't have an account");
                return;
            }

            if (i <= 0)
            {
                await ReplyAsync("Don't add negative or 0 funds.");
                return;
            }

            c.balance += i;
            await ReplyAsync(c.name + " has a new balance of " + c.balance);
            Character.update_character(c);
        }

        [Command("updateleaderboards")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task updateleaderboardsAsync()
        {
            List<Classes.Character> characters = Classes.Character.get_character();

            List<leaderboard> rtn = new List<leaderboard>();

            characters.OrderByDescending(e=>e.balance).ToList().ForEach(e=>rtn.Add(new leaderboard {name = e.name, credits = e.balance}));


            var file = "trillbot.json";

            var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(file));

            string key = secrets["api_key"];

            leaderboard_stats s = new leaderboard_stats
            {
                leaderboard = rtn,
                api_key = key
            };

            string baseurl = string.Concat("https://trilliantring.com/api/Updateleaderboard");
            // string baseurl = string.Concat ("http://localhost:5000/api/UpdateMemCount");

            var client = new RestClient(baseurl);

            var request = new RestRequest(Method.POST);
            request.AddParameter("text/json", JsonConvert.SerializeObject(s), ParameterType.RequestBody);

            request.AddHeader("Content-Type", "text/json");

            var response = client.Execute(request);

            await ReplyAsync("Stats uploaded.");
        }

        [Command("sub")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task subFundsAsync(SocketUser user, int i)
        {
            var c = Character.get_character(user.Id);
            if (c == null)
            {
                await ReplyAsync("This user doesn't have an account");
                return;
            }

            if (i <= 0)
            {
                await ReplyAsync("Don't sub negative or 0 funds.");
                return;
            }

            c.balance -= i;
            await ReplyAsync(c.name + " has a new balance of " + c.balance);
            Character.update_character(c);
        }

        [Command("bal")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task balFundsAsync(SocketUser user)
        {
            var c = Character.get_character(user.Id);
            if (c == null)
            {
                await ReplyAsync("This user doesn't have an account");
                return;
            }

            await ReplyAsync(c.name + " has a balance of " + c.balance);
            Character.update_character(c);
        }

        [Command("help")]
        public async Task helpAsync()
        {
            await Context.User.SendMessageAsync("Please check out this google document for my commands: <https://docs.google.com/document/d/1pWfIToswRCDVpqTK1Bj5Uv6s-n7zpOaqgZHQjW3SNzU/edit?usp=sharing>");
        }


        [Command("lootcrate")]
        public async Task lootCrate() {
            var emote = Context.Client.Guilds.FirstOrDefault(e=>e.Id == 493151648894681108).Emotes.ToList();
            var em = emote.ElementAt(Program.rand.Next(emote.Count));
            await ReplyAsync(Context.User.Mention + " YOU RECEIVE " + em + "    `" + em.Name + "`");
        }
    }
}