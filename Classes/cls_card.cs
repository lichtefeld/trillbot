using System.Collections.Generic;
using System.Globalization;
using System;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace trillbot.Classes {

    public partial class Card {

        [JsonProperty ("title")]
        public string title { get; set; }

        [JsonProperty ("description")]
        public string description { get; set; }

        [JsonProperty ("type")]
        public string type { get; set; }

        [JsonProperty ("Value")]
        public long value { get; set; }
    }
    public partial class Card
    {
        public static Card[] FromJson(string json) => JsonConvert.DeserializeObject<Card[]>(json, Converter.Settings);
    }

}