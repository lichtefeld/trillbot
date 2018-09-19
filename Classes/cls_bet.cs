
namespace trillbot.Classes {
    using System.Collections.Generic;
    using System.Globalization;
    using System;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;

    public class Bet {
        [JsonProperty ("ID")]
        public long Id { get; set; }

        [JsonProperty ("RacerName")]
        public string RacerName { get; set; }

        [JsonProperty ("amount")]
        public int Amount { get; set; }

        public string toString() {
            return "Bet ID: " + Id + " | Racer Name: " + RacerName + " | Amount: " + Amount;
        }

    }

    public static class SerializeBet {
        public static string ToJson (this Bet[] self) => JsonConvert.SerializeObject (self, Converter.Settings);
    }
}