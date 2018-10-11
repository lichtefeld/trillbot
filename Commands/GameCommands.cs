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
            var game = new GrandPrix {
                channelName = Context.Channel.Name
            };
            Program.games.Add(Context.Channel.Id, game);
            await ReplyAsync("Game started and bound to this channel use `ta!joingame` in this channel to join.");
        }

        [Command("startgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task startGame() {
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                //Check that a user in the game is starting the game?? Perhaps rather than the admin permission node
                await prix.startGame(Context);
            }
        }

        [Command("joingame")]
        public async Task joinGame() {
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.joinGame(Context);
            }
        }

        //TO DO COMMANDS BELOW THIS NOTE

        [Command("discard")]
        public async Task discardAsync(int i) {
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.discardAsync(Context, i);
            }
        }

        [Command("playcard")]
        public async Task playCardAsync(int i, int racerID = -1, int hazardID = -1) {
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.playCardAsync(Context, i, racerID, hazardID);
            }
        }

        [Command("ingame")]
        public async Task inGameAsync() {
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.inGameAsync(Context);
            }
        }

        [Command("reset")] //Reset the game to initial state
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task doReset() {
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.doReset(Context);
                Program.games.Remove(Context.Channel.Id);
                if(Program.games.Count == 0) {
                    await Context.Client.SetGameAsync(null, null, StreamType.NotStreaming);
                }
            }
        }

        [Command("turn")]
        public async Task whosTurnAsync() { //Need to remind a person its there turn?
            var prix = Program.games[Context.Channel.Id];
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.whosTurnAsync(Context);
            }
        }
    }
}