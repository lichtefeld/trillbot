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

namespace trillbot.Classes {

    public class Player
    {
        public List<StandardCard> hole {get;set;}
        public long ID {get;set;}
        public long cash_pool {get;set;}
        public bool fold {get;set;} = false;

    }

    public class betting_round
    {
        
    }

    public class HoldEm
    {
        private Stack<StandardCard> deck { get; set; } = StandardCard.shuffleDeck(StandardCard.straightDeck());
        public int card_round {get;set;} = 0;
        public List<StandardCard> flop {get;set;}
        public List<StandardCard> turn {get;set;}
        public List<StandardCard> river {get;set;}
        public static int big_blind {get;set;} = 500;
        public int small_blind {get;set;} = big_blind / 2;
        public int ante {get;set;}
        public int dealer_index {get;set;}
        public int min_raise {get;set;} = 50;
        public int min_bet = big_blind;
        public List<Player> players {get;set;}
    }

}
