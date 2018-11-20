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

    public class psiball_special_ability {

    }

    public class psiball_Player {
        public ulong discord_player_id { get; set; }
        public psiball_Team team { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public string psychic { get; set; }
        public psiball_special_ability ability { get; set; }
        public int space { get; set; }
        
        
    }

    public class psiball_Arena {
        

    }

}