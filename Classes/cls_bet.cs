using System.Collections.Generic;
using System.Globalization;
using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace trillbot.Classes {

    public partial class Bet {
        public long Id { get; set; }
        public string RacerName { get; set; }
        public int RacerID { get; set;}
        public int Amount { get; set; }
        public string Type { get; set; }
        public string Emote { get; set; }

        public Bet(int i, string rn, int a, string type, string emote, int ID) {
            Id = i;
            RacerName = rn;
            Amount = a;
            Type = type;
            Emote = emote;
            RacerID = ID;
        }

        public string display(IGuild Guild) {
            var emote = Guild.Emotes.ToList().FirstOrDefault(e=>e.Name == Emote);
            return "Bet ID: " + Id + " | Racer Name: " + RacerName + emote + "| Type " + Type + " | Amount: " + Amount;
        }

    }
}