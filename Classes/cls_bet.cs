    using System.Collections.Generic;
    using System.Globalization;
    using System;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;

namespace trillbot.Classes {

    public partial class Bet {
        [JsonProperty ("ID")]
        public long Id { get; set; }

        [JsonProperty ("RacerName")]
        public string RacerName { get; set; }

        [JsonProperty ("Amount")]
        public int Amount { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Emote")]
        public Discord.Emote Emote { get; set; }

        public Bet(int i, string rn, int a, string type, Discord.Emote emote) {
            Id = i;
            RacerName = rn;
            Amount = a;
            Type = type;
            Emote = emote;
        }

        public override string ToString() {
            return "Bet ID: " + Id + " | Racer Name: " + RacerName + Emote + "| Type " + Type + " | Amount: " + Amount;
        }

    }
   public partial class Bet
    {
        public static Bet[] FromJson(string json) => JsonConvert.DeserializeObject<Bet[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Bet[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
        public static string ToJson(this racer[] self) => JsonConvert.SerializeObject(self,Converter.Settings);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}