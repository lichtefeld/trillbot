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
        /* private static List<racer> racers = new List<racer>();
        private static Stack<Card> cards = new Stack<Card>();
        private static bool runningGame = false;
        private static int position = 0;
        private static int round = 1;
        private Dictionary<int, Tuple<int,int>> remedy_to_hazards = new Dictionary<int, Tuple<int,int>> {
            {0,new Tuple<int, int>(5,6)},
            {1,new Tuple<int,int>(8,9)},
            {2,new Tuple<int,int>(10,11)}
        };
        private Dictionary<int, string> target_hazard_output = new Dictionary<int, string> {
            {0, ". They are unable to move until they remove this Hazard."},
            {1, ". They better not have any other Hazards applied!"},
            {2,". They have 3 turns to remove this Hazard."},
            {3, ". They are unable to move more than two spaces!"}
        };*/

        [Command("initialize")]
        public async Task initAsync() {
            var game = new GrandPrix {
                channelName = Context.Channel.Name
            };
            Program.games.Add(Context.Channel.ID, game);
            await ReplyAsync("Game started and bound to this channel use `ta!joingame` in this channel to join.");
        }

        [Command("startgame")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task startGame() {
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                //Check that a user in the game is starting the game?? Perhaps rather than the admin permission node
                await prix.startGame(Context);
            }
        }

        [Command("joingame")]
        public async Task joinGame() {
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.joinGame(Context);
            }
        }

        //TO DO COMMANDS BELOW THIS NOTE

        [Command("discard")]
        public async Task discardAsync(int i) {
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.discardASync(Context, i);
            }
        }

        [Command("playcard")]
        public async Task playCardAsync(int i, int racerID = -1, int hazardID = -1) {
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.playCardAsync(Context, i, racerID, hazardID);
            }
        }

        [Command("ingame")]
        public async Task inGameAsync() {
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.inGameAsync(Context);
            }
        }

        [Command("reset")] //Reset the game to initial state
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task doReset() {
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.doReset(Context);
                Program.games.Remove(prix);
                if(games.Count == 0) {
                    await Context.Client.SetGameAsync(null, null, StreamType.NotStreaming);
                }
            }
        }

        [Command("turn")]
        public async Task whosTurnAsync() { //Need to remind a person its there turn?
            var prix = Program.games.FirstOrDefault(e=> e[Context.Channel.ID]);
            if (prix == null) {
                await ReplyAsync("No game running in this channel. Initialize one with `ta!initialize`");
            } else {
                await prix.whosTurnAsync(Context);
            }
        }
    }
}