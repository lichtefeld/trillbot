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

        [Command("startround")]
        public async Task StartroundAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            //perform the small and big blinds

            int small_index = game.dealer_index + 1;
            int big_index = game.dealer_index + 2;

            game.players[small_index].cash_pool = game.players[small_index].cash_pool - game.small_blind;

            game.players[big_index].cash_pool = game.players[big_index].cash_pool - game.big_blind;

            SocketGuildUser small_user = Context.Guild.GetUser(game.players[small_index].ID);
            SocketGuildUser big_user = Context.Guild.GetUser(game.players[big_index].ID);

            await ReplyAsync(generate_name(small_user) + " has been debited the small blind.");

            await ReplyAsync(generate_name(big_user) + " has been debited the big blind.");

            game.current_round = new betting_round();

            // betting_round round = new betting_round();

            game.current_round.pot += game.big_blind;
            game.current_round.pot += game.small_blind;

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

            await ReplyAsync("Cards Dealt.");

            game.current_round.call_position = game.dealer_index + 1;

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemchips")]
        public async Task HoldemchipsAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool.ToString());
        }

        [Command("holdempot")]
        public async Task HoldempotAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            await ReplyAsync(game.current_round.pot.ToString());
        }

        [Command("holdembet")]
        public async Task HoldembetAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            game.current_round.pot += amount;
            
            do
            {
                game.current_round.call_position++;
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count = 0;

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemcheck")]
        public async Task HoldemcheckAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            do
            {
                game.current_round.call_position++;
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count++;

            if(game.current_round.call_count == game.players.Count() - 1)
            {
                await lay_down_next(Context);
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemcall")]
        public async Task HoldemcallAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            int last_player_bet_amount = game.current_round.bets.Last(e => e.player_id == Context.Message.Author.Id).amount;

            int last_bet_amount = game.current_round.bets.Last().amount;

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool -= last_bet_amount - last_player_bet_amount;

            game.current_round.pot += last_bet_amount - last_player_bet_amount;

            do
            {
                game.current_round.call_position++;
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count++;

            if(game.current_round.call_count == game.players.Count() - 1)
            {
                await lay_down_next(Context);
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemraise")]
        public async Task HoldemraiseAsync(int amount)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            int last_player_bet_amount = game.current_round.bets.Last(e => e.player_id == Context.Message.Author.Id).amount;

            int last_bet_amount = game.current_round.bets.Last().amount;

            game.players.First(e => e.ID == Context.Message.Author.Id).cash_pool -= last_bet_amount - last_player_bet_amount + amount;

            game.current_round.pot += last_bet_amount - last_player_bet_amount + amount;

            do
            {
                game.current_round.call_position++;
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count = 0;

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        [Command("holdemfold")]
        public async Task HoldemfoldAsync()
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            game.players.First(e => e.ID == Context.Message.Author.Id).fold = true;

            do
            {
                game.current_round.call_position++;
            } while (!game.players[game.current_round.call_position].fold);

            game.current_round.call_count++;

            if(game.current_round.call_count == game.players.Count() - 1)
            {
                await lay_down_next(Context);
                return;
            }

            await ReplyAsync("Bet goes to " + generate_name(get_usr_from_index(Context, game.current_round.call_position)));
        }

        private static async Task lay_down_next(SocketCommandContext context)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == context.Channel.Id).Value;

            if (game.current_round.flop == null)
            {
                game.current_round.flop = new List<StandardCard>();

                game.current_round.flop.Add(game.deck.Pop());
                game.current_round.flop.Add(game.deck.Pop());
                game.current_round.flop.Add(game.deck.Pop());

                List<string> message = new List<string>();

                message.Add("The Flop:");

                game.current_round.flop.ForEach(e => message.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner);

                return;
            }

            if (game.current_round.turn == null)
            {
                game.current_round.turn = game.deck.Pop();

                List<string> message = new List<string>();

                message.Add("The Turn:");

                message.Add(StandardCard.value_to_output[game.current_round.turn.value].ToString() + " of " + StandardCard.suit_to_output[game.current_round.turn.suit].ToString());

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner);

                return;
            }

            if (game.current_round.river == null)
            {
                game.current_round.river = game.deck.Pop();

                List<string> message = new List<string>();

                message.Add("The River:");

                message.Add(StandardCard.value_to_output[game.current_round.river.value].ToString() + " of " + StandardCard.suit_to_output[game.current_round.river.suit].ToString());

                string rtner = string.Join(System.Environment.NewLine, message);

                await context.Channel.SendMessageAsync(rtner);

                return;
            }
        }

        private static SocketGuildUser get_usr_from_index(SocketCommandContext context, int index)
        {
            HoldEm game = Program.HoldEm.First(e => e.Key == context.Channel.Id).Value;

            SocketGuildUser usr = context.Guild.GetUser(game.players[index].ID);

            return usr;
        }

        private static string generate_name(SocketGuildUser usr)
        {
            return usr.Nickname == null ? usr.Username : usr.Nickname;
        }
        private async Task send_cards(UInt64 id)
        {
            SocketGuildUser usr = Context.Guild.GetUser(id);

            List<string> message = new List<string> { "Your Hole Cards:" };

            HoldEm game = Program.HoldEm.First(e => e.Key == Context.Channel.Id).Value;

            Player player = game.players.First(e => e.ID == id);

            player.hole.ForEach(e => message.Add(StandardCard.value_to_output[e.value].ToString() + " of " + StandardCard.suit_to_output[e.suit].ToString()));

            await usr.SendMessageAsync(string.Join(System.Environment.NewLine, message), false, null, null);
        }


    }
}
