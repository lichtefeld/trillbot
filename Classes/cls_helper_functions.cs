using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading;

namespace trillbot.Classes
{
    public class helpers

    {
        public static string parseEmote(string s) {
            string temp = s.Substring(s.IndexOf(":")+1);
            return ":" + temp.Substring(0,temp.IndexOf(":")+1);
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
                embed.AddField(property,helpers.GetPropValue(obj,property),true);
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

        /** @fn          output
          * @brief       This function handles outputting a list of strings seperated by a seperator character. The resulting strings are sent to the provided Discord Channel
          * @param[in] 1 ISocketMessageChannel. A discord channel to send the messages back out to. 
          * @param[in] 2 List<string>. A list of strings to be appended together for output.
          * @param[in] 3 string. The string to be appended onto the list of strings to combine them. Default: NewLine character.
          * @note        This function is syncronous in operation
          */
        public static void output(ISocketMessageChannel channel, List<string> str, string seperator = "\n") {
            int count = 0;
            string output_string = "";
            if (str.Count == 0) return; 
            foreach(string s in str) {
                count += s.Length + seperator.Length;
                if (count >= 2000) {
                    channel.SendMessageAsync(output_string);
                    Thread.Sleep (100);
                    count = s.Length + seperator.Length;
                    output_string = s + seperator;
                } else {
                    output_string += s + seperator;
                }
            }
            channel.SendMessageAsync(output_string).GetAwaiter().GetResult();
        }
        public static void output(IUser User, List<string> str) {
            int count = 0;
            string output_string = "";
            if (str.Count == 0) return; 
            foreach(string s in str) {
                count += s.Length + 1;
                if (count >= 2000) {
                    User.SendMessageAsync(output_string);
                    count = s.Length;
                    output_string = s + System.Environment.NewLine;
                } else {
                    output_string += s + System.Environment.NewLine;
                }
            }
            User.SendMessageAsync(output_string).GetAwaiter().GetResult();
        }

        public static void output(IUser User, string str) {
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
                helpers.output(User, output);
                str = str.Remove(0,split);
                helpers.output(User,str);
            } else {
                User.SendMessageAsync(str).GetAwaiter().GetResult();
            }
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

    }

}