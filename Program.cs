using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace trillbot
{
    internal class Program
    {
        private static void Main() => new Program().RunBotAsync().GetAwaiter().GetResult();
        public static Random rand = new Random();
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        public static readonly string[] prefixes = {
            "ta!"
        };

        public static Dictionary<ulong,Classes.GrandPrix> games = new Dictionary<ulong, Classes.GrandPrix>(); 

        public async Task RunBotAsync()
        {

            var file = "trillbot.json";

            var secrets = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(file));

            _client = new DiscordSocketClient();
            _commands = new CommandService();
            _services = new ServiceCollection().AddSingleton(_client).AddSingleton(_commands).BuildServiceProvider();

            //event subscriptions
            _client.Log += Log;

            await RegisterCommandAsync();

            await _client.LoginAsync(TokenType.Bot, secrets["bot_code"]);

            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private static Task Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;

            //prevent infinite loops where the bot is talking to itself or other bots
            if (message is null || message.Author.IsBot)
            {
                return;
            }

            if (message.Content.Contains('“') || message.Content.Contains('”'))
            {
                var msg = message.Content.Replace('“', '"').Replace('”', '"');
                await message.ModifyAsync(e => e.Content = msg);
            }

            var msg_prefix = message.Content.ToString().Substring(0, 3);

            //if the prefix is in the list of valid prefixes, continue
            if (prefixes.Where(e => String.Equals(e, msg_prefix, StringComparison.OrdinalIgnoreCase)).Count() > 0)
            {
                //log that we have a command sent
                var logmessage = String.Concat(message.Author, " sent command ", message.Content);

                await Log(new LogMessage(LogSeverity.Info, "VERBOSE", logmessage));

                var argPosition = 0;

                if (message.HasStringPrefix(msg_prefix, ref argPosition) || message.HasMentionPrefix(_client.CurrentUser, ref argPosition))
                {
                    var context = new SocketCommandContext(_client, message);

                    var server_id = context.Guild.Id.ToString();

                    var result = await _commands.ExecuteAsync(context, argPosition, _services);
                    if (!result.IsSuccess)
                    {
                        var channel = context.Guild.Channels.FirstOrDefault(e => e.Name == "bot-commands") as ISocketMessageChannel;

                        if (channel != null) await channel.SendMessageAsync(result.ErrorReason);

                        Console.WriteLine(result.ErrorReason);
                    }
                }
            }

        }
    }
}