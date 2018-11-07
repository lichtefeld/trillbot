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

    }

    public class betting_round
    {
        
    }

    public class HoldEm
    {
        private Stack<StandardCard> deck { get; set; } = new Stack<StandardCard>();

        public List<StandardCard> flop {get;set;}

        public List<StandardCard> turn {get;set;}

        public List<StandardCard> river {get;set;}
    }

}
