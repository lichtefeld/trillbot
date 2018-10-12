using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;

namespace trillbot.Classes
{
    public class helpers

    {
        public static object GetPropValue(object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static Embed ObjToEmbed(object obj, string title_property_name = "")
        {
            var properties = obj.GetType().GetProperties().Select(e=>e.Name).ToArray();
            var embed = new EmbedBuilder();

            foreach (var property in properties)
            {                
                embed.WithTitle(helpers.GetPropValue(obj,title_property_name).ToString());
                embed.AddInlineField(property,helpers.GetPropValue(obj,property));
            }

            return embed.Build();
        }

        public static string center(string s, int i) {
            bool odd = false;
            string spaces = "";
            int toCenter = i - s.Length;
            if (toCenter % 2 == 1) odd = true;
            for(int j = 0; j < toCenter/2; j++) {
                spaces += " ";
            }
            if(odd) {
                return " " + spaces + s + spaces;
            } else {
                return spaces + s + spaces;
            }
        }
    }

}