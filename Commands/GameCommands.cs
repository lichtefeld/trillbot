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
        private List<racer> racers = new List<racer>();
        private Stack<Card> cards = new Stack<Card>();

        [Command("startgame")]
        public async Task startGame()
        {
            cards = this.generateDeck();
            foreach(racer r in racers) {
                for(int i = 0; i < 8; i++) {
                    if(!cards.Any()) { cards = generateDeck(); }
                    r.cards.Add(cards.Pop());
                }
            }

            await ReplyAsync("Game Started...");
        }

        [Command("joingame")]
        public async Task joinGame() {
            racer racer = racer.get_racer(Context.Message.Author.Id);

            if(racer == null) {
                await ReplyAsync("No racer found for you");
            } else {
                racers.Add(racer);
            }

            await ReplyAsync("You have joined the game");
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

        private Stack<Card> shuffleDeck(List<Card> cards) {
            Stack<Card> s = new Stack<Card>();

            while (!cards.Any()) {
                int num = trillbot.Program.rand.Next(cards.Count);
                s.Push(cards[num]);
                cards.RemoveAt(num);
            }

            return s;
        }

    }
}