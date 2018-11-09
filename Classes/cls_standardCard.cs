using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using JsonFlatFileDataStore;
using Newtonsoft.Json;
using trillbot.Classes;

namespace trillbot.Classes
{

    public class StandardCard
    {

        //
        public int value { get; set; }
        public int suit { get; set; }

        public StandardCard(int v, int s)
        {
            value = v;
            suit = s;
        }

        public static readonly Dictionary<int, string> value_to_output = new Dictionary<int, string>() {
            {2,"2"},
            {3,"3"},
            {4,"4"},
            {5,"5"},
            {6,"6"},
            {7,"7"},
            {8,"8"},
            {9,"9"},
            {10,"10"},
            {11, "J"},
            {12,"Q"},
            {13,"K"},
            {14,"A"}
        };

        public static readonly Dictionary<int, string> suit_to_output = new Dictionary<int, string>() {
            {1,"♤"},
            {2,"♧"},
            {3,"♡"},
            {4,"♢"}
        };

        public override string ToString()
        {
            return value_to_output[value] + suit_to_output[suit];
        }

        public static List<StandardCard> straightDeck()
        {
            List<StandardCard> cards = new List<StandardCard>();
            for (int i = 1; i < 5; i++)
            {
                for (int k = 2; k < 15; k++)
                {
                    cards.Add(new StandardCard(k, i));
                }
            }
            return cards;
        }

        public static Stack<StandardCard> shuffleDeck(List<StandardCard> deck)
        {
            Stack<StandardCard> shuffled = new Stack<StandardCard>();
            while (deck.Count > 0)
            {
                var card = deck[Program.rand.Next(deck.Count)];
                shuffled.Push(card);
                deck.Remove(card);
            }
            return shuffled;
        }

        public static int eval_hand (List<StandardCard> eval)
        {
            int weight = 0;
            
            weight = ContainsPairOrTwoPair(eval);
            weight = ContainsStraightFlush(eval);
            weight = ContainsThreeOfAKind(eval);
            weight = ContainsStraight(eval);
            weight = ContainsFlush(eval);
            weight = ContainsFullHouse(eval);
            weight = ContainsFourOfAKind(eval);
            weight = ContainsStraightFlush(eval);
            weight = ContainsRoyalFlush(eval);

            return weight;
        }

        public static bool contains_all_same_suit(List<StandardCard> eval)
        {
            StandardCard FirstCard = eval.First();

            return eval.Count(e => e.suit == FirstCard.suit) == 5 ? true : false;
        }

        public static bool contains_five_sequential(StandardCard[] eval)
        {
            return eval.Zip(eval.Skip(1), (a, b) => (a.value + 1) == b.value).All(x => x);
        }

        public static int ContainsRoyalFlush(List<StandardCard> eval)
        {
            int weight = 0;

            if(contains_all_same_suit(eval) && contains_five_sequential(eval.ToArray()) && eval.OrderBy(e=>e.value).Last().value ==14) weight = 10;

            return weight;
        }

        public static int ContainsStraightFlush(List<StandardCard> eval)
        {
            int weight = 0;

            if(contains_all_same_suit(eval) && contains_five_sequential(eval.ToArray())) weight = 9;

            return weight;
        }

        public static int ContainsFourOfAKind(List<StandardCard> eval)
        {
            int weight = 0;

            foreach(StandardCard card in eval)
            {
                if(eval.Count(e=>e.value == card.value) == 4)
                {
                    weight = 8;
                    return weight;
                }
            }

            return weight;
        }

        public static int ContainsFullHouse(List<StandardCard> eval)
        {
            int weight = 0;
            bool threes = false;
            bool pair = false;

            foreach(StandardCard card in eval)
            {
                if(eval.Count(e=>e.value == card.value) == 3)
                {
                    threes = true;
                }
                if(eval.Count(e=>e.value == card.value) == 2)
                {
                    pair = true;
                }
            }

            if(threes && pair)
            {
                weight = 7;
            }

            return weight;
        }

        public static int ContainsFlush(List<StandardCard> eval)
        {
            int weight = 0;

            if(contains_all_same_suit(eval)) weight = 6;

            return weight;
        }

        public static int ContainsStraight(List<StandardCard> eval)
        {
            int weight = 0;

            if(contains_five_sequential(eval.ToArray())) weight = 5;

            return weight;
        }

        public static int ContainsThreeOfAKind(List<StandardCard> eval)
        {
            int weight = 0;
            bool threes = false;

            foreach(StandardCard card in eval)
            {
                if(eval.Count(e=>e.value == card.value) == 3)
                {
                    threes = true;
                }
            }

            if(threes) weight = 4;

            return weight;

        }

        public static int ContainsPairOrTwoPair(List<StandardCard> eval)
        {
            int weight = 0;
            int paircount = 0;
            int firstpair_val = 0;

            foreach(StandardCard card in eval)
            {
                if(eval.Count(e=>e.value == card.value) == 2)
                {
                    if(card.value != firstpair_val)
                    {
                        firstpair_val = card.value;
                        paircount++;
                    }
                }
            }

            if(paircount == 2) weight = 3;

            if(paircount == 1) weight = 2;

            return weight;

        }

    }
}