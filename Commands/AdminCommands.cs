using System;
using System.Threading.Tasks;
using Discord;
using System.Collections.Generic;
using System.Linq;
using trillbot.Classes;
using Discord.Commands;

namespace trillbot.Commands
{

    public class AdminCommands : ModuleBase<SocketCommandContext>
    {

        [Command("help")]
        public async Task helpAsync()
        {
            await Context.User.SendMessageAsync("Please check out this google document for my commands: <https://docs.google.com/document/d/1pWfIToswRCDVpqTK1Bj5Uv6s-n7zpOaqgZHQjW3SNzU/edit?usp=sharing>");
        }

        [Command("addRace")]
        public async Task addBettingRaceAsync(int winAmount) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            race r = new race();
            r.totalPool[0] = winAmount;
            r.totalPool[1] = winAmount * 2 / 3;
            r.totalPool[2] = winAmount / 3;
            race.insert_race(r);
            await Context.Channel.SendMessageAsync("Race Added");
        }

        [Command("addRacer")]
        public async Task addRacertoRaceAsync(int id, int racer) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            race r = race.get_race(id);
            if ( r == null ) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", this race ID doesn't exist.");
                return;
            }
            racer rr = Classes.racer.get_racer(racer);
            if ( rr == null ) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", this racer ID doesn't exist.");
                return;
            }
            r.racersWithBets.Add(new racerBet(racer));
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", added " + rr.nameID() + " to the race!");
            r.updatePayouts();
            race.update_race(r);
        }

        [Command("removeRacer")]
        public async Task removeRacerFromRaceAsync() {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
        }

        [Command("pauseBet")]
        public async Task pauseBettingAsync(int ID) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            race r = race.get_race(ID);
            if ( r == null ) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", this race ID doesn't exist.");
                return;
            }
            r.acceptingBets = false;
            race.update_race(r);
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", bets paused");
        }

        [Command("payoutRace")]
        public async Task payoutRaceAsync(int ID, int win, int place, int show) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            race r = race.get_race(ID);
            if ( r == null ) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", this race ID doesn't exist.");
                return;
            }
            r.makePayouts(win,place,show,Context);
            await Context.Channel.SendMessageAsync("Payouts Complete.");
        }

        [Command("house")]
        public async Task houseBalAsync() {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", the house has a balance of " + s.houseBal + " imperial credits.");
        }
        
        [Command("removeRacingChannel")]
        public async Task removeRacingChannelAsync() {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            s.racingChannelSnowflakes.Remove(Context.Channel.Id);
            Server.replace_Server(s);
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", removed " + Context.Channel.Name + ", from the racing channels on this server.");
        }

        [Command("addRacingChannel")]
        public async Task addRacingChannelAsync() {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            s.racingChannelSnowflakes.Add(Context.Channel.Id);
            Server.replace_Server(s);
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", added " + Context.Channel.Name + ", to the racing channels on this server.");
        }

        [Command("removeAuthorized")]
        public async Task removeAuthorizedAsync(IGuildUser User) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            s.adminSnowflakes.Remove(User.Id);
            Server.replace_Server(s);
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", removed " + User.Mention + ", from the authorized users on this server.");
        }

        [Command("addAuthorized")]
        public async Task addAuthorizedAsync(IGuildUser User) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            s.adminSnowflakes.Add(User.Id);
            Server.replace_Server(s);
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", added " + User.Mention + ", to the authorized users on this server.");
        }

        [Command("showRacingVersions")]
        public async Task showRacingVersionAsync() {
            var tV = textVersion.get_textVersion();
            var str = new List<string>();
            var count = 21;
            str.Add("**Special Abilities**");
            for(int i = 0; i < tV.Count; i++) {
                var s = "**#" + (i) + ":** " + tV[i].name + " - *" +tV[i].desc + "*";
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

        [Command("setRacingVersion")]
        public async Task setRacingVersionAsync(int i) {
            Server s = Server.get_Server(Context.Guild.Id);
            if (s == null) { 
                createServerObject(Context); 
                s = Server.get_Server(Context.Guild.Id);
            }
            if (!s.isAdmin(Context.Guild.GetUser(Context.User.Id))) {
                await Context.Channel.SendMessageAsync(Context.User.Mention + ", you aren't listed as an authorized user for this server.");
                return;
            }
            s.racingVersionDefault = i;
            Server.replace_Server(s);
            await Context.Channel.SendMessageAsync(Context.User.Mention + ", the default racing version has been set to id " + i );
        }

        private void createServerObject(SocketCommandContext Context) {
            Server s = new Server();
            s.snowflake = Context.Guild.Id;
            s.Title = Context.Guild.Name;
            s.adminSnowflakes.Add(Context.Guild.OwnerId);
            s.startingBalance = 1000000;
            Server.insert_Server(s);
        }
    }
}