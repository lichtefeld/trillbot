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
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        [Command("initialize")]
        public async Task initAsync() {
            if (Program.games.TryGetValue(Context.Channel.Id, out  GrandPrix game)) {
                await ReplyAsync("Woah there, a game is already initialized in this channel. Try using `ta!reset` to reset this channel");
                return;
            }
            game = new GrandPrix {
                channelName = Context.Channel.Name
            };
            Program.games.Add(Context.Channel.Id, game);
            await ReplyAsync("Game started and bound to this channel use `ta!joingame` in this channel to join.");
        }

        [Command("startgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task startGame() {
            try { 
                var prix = Program.games[Context.Channel.Id];
                await prix.startGame(Context); //Consider checking for a player to be in the game, rather than the Administrator node
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }

        [Command("joingame")]
        public async Task joinGame() {
            try { 
                var prix = Program.games[Context.Channel.Id];
                await prix.joinGame(Context);
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }

        [Command("discard")]
        public async Task discardAsync(int i) {
            try { 
                var prix = Program.games[Context.Channel.Id];
                await prix.discardAsync(Context, i);
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }

        [Command("playcard")]
        public async Task playCardAsync(int i, int racerID = -1, int hazardID = -1) {
            try { 
                var prix = Program.games[Context.Channel.Id];
                await prix.playCardAsync(Context, i, racerID, hazardID);
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }

        [Command("ingame")]
        public async Task inGameAsync() {
            try { 
                var prix = Program.games[Context.Channel.Id];
                await prix.inGameAsync(Context);
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }

        [Command("reset")] //Reset the game to initial state
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task doReset() {
            try {
                var prix = Program.games[Context.Channel.Id];
                await prix.doReset(Context);
                Program.games.Remove(Context.Channel.Id);
                if(Program.games.Count == 0) {
                    await Context.Client.SetGameAsync(null, null, StreamType.NotStreaming);
                }
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }

        [Command("turn")]
        public async Task whosTurnAsync() { //Need to remind a person its there turn?
            try { 
                var prix = Program.games[Context.Channel.Id];
                await prix.whosTurnAsync(Context);
            } catch {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            }
        }
    }
}