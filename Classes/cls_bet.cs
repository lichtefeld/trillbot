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
        public int Id { get; set; }
        public string RacerName { get; set; }
        public int RacerID { get; set;}
        public int Amount { get; set; }
        public string Type { get; set; }
        public int raceID { get; set;}
        public int characterID { get; set;}

        public Bet(string rn, int a, string type, int ID, int rID, int charID) {
            RacerName = rn;
            Amount = a;
            Type = type;
            RacerID = ID;
            raceID = rID;
            characterID = charID;
        }

        public string display() {
            return "Bet. Racer Name: " + RacerName  + "| Type " + Type + " | Amount: " + Amount;
        }

    }
}