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
    public class CharacterCreation : ModuleBase<SocketCommandContext>
    {
        [Command("registeraccount")]
        public async Task NewcharacterAsync(params string[] args)
        {
            SocketGuildUser usr = Context.Guild.GetUser(Context.Message.Author.Id);
            Classes.Character c = Character.get_character(Context.Message.Author.Id);
            if (c != null) { 
                await ReplyAsync("You already have an account!");
                return;
            }


            string name = usr.Nickname != null ? usr.Nickname : usr.Username;
            
            Classes.Character character = new Character
            {
                name = name
            };

            character.player_discord_id = Context.Message.Author.Id;

            Character.insert_character(character);

            await ReplyAsync(name + ", you have created an account. You can now use ta!bet <racer> <amount>");

        }

        [Command("deleteaccount")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task DeletecharacterAsync(params string[] args)
        {
            Classes.Character character = Character.get_character(Context.Message.Author.Id);

            if(character == null) {
                await ReplyAsync("No character found for you");
            } else {

                Classes.Character.delete_character(character);
                await ReplyAsync("Account Deleted.");
            }
        }

        [Command("listaccounts")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ListcharactersAsync()
        {
            List<Classes.Character> characters = Character.get_character();

            await ReplyAsync("**Racers for the Grand Prix**"+System.Environment.NewLine+string.Join(System.Environment.NewLine,characters.Select(e=>e.name).ToList()));
        }
    }
}
