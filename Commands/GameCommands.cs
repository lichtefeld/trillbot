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
using trillbot;

namespace trillbot.Commands
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        [Command("initialize")]
        public async Task initAsync() {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if (prix.Value != null) {
                await Context.Channel.SendMessageAsync("Woah there, a game is already initialized in this channel. Try using `ta!reset` to reset this channel");
                return;
            }
            var game = new GrandPrix {
                channelName = Context.Channel.Name
            };
            Program.games.Add(Context.Channel.Id, game);
            await Context.Channel.SendMessageAsync("Game started and bound to this channel use `ta!joingame` in this channel to join.");
        }

        [Command("startgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task startGame() {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                prix.Value.startGame(Context); //Consider checking for a player to be in the game, rather than the Administrator node
            }
        }

        [Command("joingame")]
        public async Task joinGame() {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.joinGame(Context);
            }
        }

        [Command("discard")]
        public async Task discardAsync(int i) {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.discardAsync(Context, i);
            }
        }

        [Command("playcard")]
        public async Task playCardAsync(int i, int racerID = -1, int hazardID = -1) {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.playCardAsync(Context, i, racerID,hazardID);
            }
        }

        [Command("ingame")]
        public async Task inGameAsync() {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.inGameAsync(Context);
            }
        }

        [Command("reset")] //Reset the game to initial state
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task doReset() {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.doReset(Context);
                trillbot.Program.games.Remove(Context.Channel.Id);
                if(Program.games.Count == 0) {
                    await Context.Client.SetGameAsync(null, null, StreamType.NotStreaming);
                }
            }
        }

        [Command("turn")]
        public async Task whosTurnAsync() { //Need to remind a person its there turn?
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.whosTurnAsync(Context);
            }
        }

        [Command("showhand")]
        public async Task DisplayRacerHandAsync() {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else { 
                prix.Value.showHand(Context);
            }
        }

        [Command("playability")]
        public async Task playAbilityAsync(int i = -1, string[] j = null) {
            var prix = Program.games.ToList().FirstOrDefault(e=> e.Key == Context.Channel.Id);
            if ( prix.Value == null) {
                await Context.Channel.SendMessageAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                int[] k = new int[j.Length];
                for(int z = 0; z < j.Length; z++) {
                    int l;
                    if (Int32.TryParse(j[z],out l)) {
                        k[z] = l;
                    } else {
                        await Context.Channel.SendMessageAsync("You didn't pass in integers. Please try again");
                        return;
                    }
                }
                prix.Value.playAbility(Context, i, k);
            }
        }

        [Command("shuffleRacers")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task shuffleRacersAsync(int i) {
            var names = new List<string>();
            for(int j = 0; j < i; j++) {
                names.Add("Racer " + i);
            }
            var shuffledNames = new List<string>();
            while(names.Count != 0) {
                int j = Program.rand.Next(names.Count);
                shuffledNames.Add(names[j]);
                names.RemoveAt(j);
            }
            await ReplyAsync(String.Join(System.Environment.NewLine, shuffledNames));
        }
    }
}