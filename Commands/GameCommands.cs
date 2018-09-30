using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using RestSharp;
using trillbot.Classes;

namespace trillbot.Commands
{
    public class GameCommands : ModuleBase<SocketCommandContext>
    {
        [Command("hand")]
        public async Task DisplayCardsAsyn(string racerName, int amount)
        {
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            racer racer = racer.get_racer(Context.Message.Author.Id);

            if (racer == null)
            {
                await ReplyAsync("I don't think you are a racer");
                return;
            }
            

        }

    }
}