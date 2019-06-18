using Newtonsoft.Json;

namespace trillbot.Classes
{
    public static class Serialize
    {
        public static string ToJson(this racer[] self) => JsonConvert.SerializeObject(self, Converter.Settings);
    }
}
