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
        public static string parseEmote(string s) {
            var temp = s.Substring(s.IndexOf(":")+1);
            return temp.Substring(0,s.IndexOf(":"));
        }

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
            string spaces = "";
            int toCenter = i - s.Length;
            for(int j = 0; j < toCenter/2; j++) {
                spaces += " ";
            }
            if(toCenter % 2 == 1) {
                return " " + spaces + s + spaces;
            } else {
                return spaces + s + spaces;
            }
        }

        public static void output(ISocketMessageChannel channel, List<string> str) {
            int count = 0;
            string output_string = "";
            if (str.Count == 0) return; 
            foreach(string s in str) {
                count += s.Length + 1;
                if (count >= 2000) {
                    channel.SendMessageAsync(output_string);
                    count = s.Length;
                    output_string = s + System.Environment.NewLine;
                } else {
                    output_string += s + System.Environment.NewLine;
                }
            }
            channel.SendMessageAsync(output_string).GetAwaiter().GetResult();
        }

        public static void UpdateRacersDatabase() {
            Serialize.ToJson(trillbot.Commands.RacerCreation.allRacers.ToArray());
        }

        public static void UpdateRacersList() {
            trillbot.Commands.RacerCreation.allRacers = racer.get_racer().OrderBy(e=>e.ID).ToList();;
        }

        public static void output(ISocketMessageChannel channel, string str) {
            if (str.Length == 0) return;
            if (str.Length > 2000) {
                int split = 0;
                for(int i = 2000; i > 0; i--) {
                    if(str[i] == ' ') {
                        split = i;
                        break;
                    }
                }
                string output = str.Remove(split);
                helpers.output(channel, output);
                str = str.Remove(0,split);
                helpers.output(channel,str);
            } else {
                channel.SendMessageAsync(str).GetAwaiter().GetResult();
            }
        }

        public static string formatBets(Character character) {
            var output = new List<string>();
            output.Add("**" +character.name + " Bets** ```");
            foreach (trillbot.Classes.Bet bet in character.bets) {
                output.Add(bet.ToString());
            }
            output.Add("```");
            return String.Join(System.Environment.NewLine,output);
        }
    }

}