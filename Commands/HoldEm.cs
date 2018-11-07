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
    public class PlayHoldem : ModuleBase<SocketCommandContext>
    {
        [Command("startholdem")]
        public async Task StartholdemAsync()
        {
            HoldEm game = new HoldEm();

            Program.HoldEm.Add(Context.Channel.Id, game);

            await ReplyAsync("Game started. Use the joinholdem game to grab a seat at the table.");
        }

        [Command("joinholdem")]
        public async Task JoinholdemAsync()
        {
            HoldEm Game = Program.HoldEm.FirstOrDefault(e => e.Key == Context.Channel.Id).Value;

            if (Game == null)
            {
                await ReplyAsync("A game has not been started in this channel. Use the startholdem command to start a game.");

                return;
            }

            Player player = new Player
            {
                cash_pool = 10000,
                ID = Context.Message.Author.Id
            };

            Game.players.Add(player);

            string message = Context.Message.Author.Username + " has joined the game.";
        }

        [Command("playholdem")]
        public async Task PlayholdemAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            //perform the small and big blinds

            int small_index = game.dealer_index + 1;
            int big_index = game.dealer_index + 2;

            game.players[small_index].cash_pool = game.players[small_index].cash_pool - game.small_blind;

            game.players[big_index].cash_pool = game.players[big_index].cash_pool - game.big_blind;

            betting_round round = new betting_round();

            round.pot += game.big_blind;
            round.pot += game.small_blind;

            game.dealer_index++;

            //deal the hole cards

            Stack<StandardCard> deck = game.deck;
            int max_deal = game.players.Count * 2;

            for (int i = 0; i < 2; i++)
            {
                foreach (var player in game.players)
                {
                    player.hole.Add(deck.Pop());
                    
                }
            }

            game.players.ForEach(async e => await send_cards(e.ID));
        }

        private async Task start_betting_round ()
        {           
            //perform the small and big blinds

            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            
            int callcount = 0;

            do
            {
                
            } while (callcount < game.players.Count);
        }
        private async Task send_cards(UInt64 id)
        {
            SocketGuildUser usr = Context.Guild.GetUser(id);

            List<string> message = new List<string> {"Your Hole Cards:"};

            HoldEm game = Program.HoldEm.First(e=>e.Key == Context.Channel.Id).Value;

            Player player = game.players.First(e=>e.ID == id);

            player.hole.ForEach(e=>message.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

            await usr.SendMessageAsync(string.Join(System.Environment.NewLine, message),false,null,null);
        }


    }
}
