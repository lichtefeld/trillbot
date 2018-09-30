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
        public long Amount { get; set; }

        public override string ToString() {
            return "Bet ID: " + Id + " | Racer Name: " + RacerName + " | Amount: " + Amount;
        }

    }
   public partial class Bet
    {
        public static Bet[] FromJson(string json) => JsonConvert.DeserializeObject<Bet[]>(json, Converter.Settings);
    }

    public static class Serialize
    {
        public static string ToJson(this Bet[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
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