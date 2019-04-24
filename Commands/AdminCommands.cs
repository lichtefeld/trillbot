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
using trillbot.Classes;
using RestSharp;

namespace trillbot.Commands
{

    public class AdminCommands : ModuleBase<SocketCommandContext>
    {

        [Command("help")]
        public async Task helpAsync()
        {
            await Context.User.SendMessageAsync("Please check out this google document for my commands: <https://docs.google.com/document/d/1pWfIToswRCDVpqTK1Bj5Uv6s-n7zpOaqgZHQjW3SNzU/edit?usp=sharing>");
        }

    }
}