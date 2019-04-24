using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace trillbot.Classes
{
    public static class Serialize
    {
        public static string ToJson(this racer[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
