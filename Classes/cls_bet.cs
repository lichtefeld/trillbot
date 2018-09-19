
namespace trillbot.Classes {
    using System.Collections.Generic;
    using System.Globalization;
    using System;
    using Newtonsoft.Json.Converters;
    using Newtonsoft.Json;

    public partial class Bet {
        [JsonProperty ("ID")]
        public long Id { get; set; }

        [JsonProperty ("RacerName")]
        public string RacerName { get; set; }

        [JsonProperty ("amount")]
        public int Amount { get; set; }

    }

    public static class SerializeBet {
        public static string ToJson (this Bet[] self) => JsonConvert.SerializeObject (self, Converter.Settings);
    }
}