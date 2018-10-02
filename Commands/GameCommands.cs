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
using RestSharp;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        private static List<racer> racers = new List<racer>();
        private static Stack<Card> cards = new Stack<Card>();
        private bool endGame = false;

        [Command("startgame")]
        public async Task startGame() {
            await ReplyAsync("Game Started");
            this.doWorkAsyncInfiniteLoop();
        }

        [Command("joingame")]
        public async Task joinGame() {
            racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
                return;
            } else {
                foreach (racer r in racers) {
                    if(r.ID == racer.ID) {
                        await ReplyAsync("You have already joined the game!");
                        return;
                    }
                }
                racers.Add(racer);
            }

            await ReplyAsync("You have joined the game");
        }

        [Command("dealDeck")]
        public async Task dealCards()
        {
            cards = this.generateDeck();
            foreach(racer r in racers) {
                for(int i = 0; i < 8; i++) {
                    if(cards.Count == 0) { cards = this.generateDeck(); }
                    r.cards.Add(cards.Pop());
                }
                racer.update_racer(r);
            }

            await ReplyAsync("Cards Dealt");
        }
        [Command("reset")]
        public async Task doReset() {
            cards = new Stack<Card>();
            foreach (racer r in racers) {
                r.resetCards();
            }
            racers = new List<racer>();
            await ReplyAsync("Game Reset");
        }

        private async Task doWorkAsyncInfiniteLoop() {
            while(true) {
                if(endGame) {
                    break;
                }
                await Task.Delay(200);
            }
        }

        private Stack<Card> generateDeck() {
            return shuffleDeck(freshDeck());
        }

        private List<Card> freshDeck() {
            List<Card> c = new List<Card>();
            List<Card> temp = trillbot.Classes.Card.get_card();

            foreach (Card c1 in temp) {
                for(int i = 0; i < c1.count; i++) {
                    c.Add(c1);
                }
            }

            return c;
        }

        private Stack<Card> shuffleDeck(List<Card> c) {
            Stack<Card> s = new Stack<Card>();

            while (c.Count > 0) {
                int num = trillbot.Program.rand.Next(0,c.Count);
                s.Push(c[num]);
                c.RemoveAt(num);
            }

            return s;
        }

    }
}