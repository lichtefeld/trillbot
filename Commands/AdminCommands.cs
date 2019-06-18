using System.Threading.Tasks;
using Discord;
using Discord.Commands;

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