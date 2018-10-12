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

namespace trillbot.Commands
{
    public class CharacterCreation : ModuleBase<SocketCommandContext>
    {
        [Command("registeraccount")]
        public async Task NewcharacterAsync()
        {
            var usr = Context.Guild.GetUser(Context.Message.Author.Id);
            var c = Character.get_character(Context.Message.Author.Id);
            if (c != null) { 
                await ReplyAsync("You already have an account!");
                return;
            }

            var name = usr.Nickname != null ? usr.Nickname : usr.Username;
            
            c = new Character
            {
                name = name
            };

            c.player_discord_id = Context.Message.Author.Id;

            Character.insert_character(c);

            await ReplyAsync(name + ", you have created an account. You can now use ta!bet <racer> <amount>");

        }

        [Command("deleteaccount")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeletecharacterAsync()
        {
            var c = Character.get_character(Context.Message.Author.Id);

            if(c == null) {
                await ReplyAsync("No character found for you");
            } else {

                Classes.Character.delete_character(c);
                await ReplyAsync("Account Deleted.");
            }
        }

        [Command("listaccounts")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListcharactersAsync()
        {
            var characters = Character.get_character();

            await ReplyAsync("**Racers for the Grand Prix**"+System.Environment.NewLine+string.Join(System.Environment.NewLine,characters.Select(e=>e.name).ToList()));
        }
    }
}
