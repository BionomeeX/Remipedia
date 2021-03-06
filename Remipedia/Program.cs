using Remipedia.Modules;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Remipedia.TypeReader;

namespace Remipedia
{
    class Program
    {
        public static async Task Main()
            => await new Program().MainAsync();


        private readonly DiscordSocketClient _client = new(new DiscordSocketConfig
        {
            LogLevel = LogSeverity.Verbose,
        });
        private readonly CommandService _commands = new();

        private Program()
        {
            // Image sent on Discord are placed in an "Inputs" folder
            if (Directory.Exists("Inputs"))
            {
                foreach (var file in Directory.GetFiles("Inputs"))
                {
                    File.Delete(file);
                }
            }
            else
            {
                Directory.CreateDirectory("Inputs");
            }
            // Output images by the model are sometimes placed at the root
            foreach (var file in Directory.GetFiles(Directory.GetCurrentDirectory()))
            {
                if (file.EndsWith(".jpg"))
                {
                    File.Delete(file);
                }
            }

            _client.Log += (msg) =>
            {
                Console.WriteLine(msg);
                return Task.CompletedTask;
            };
            _commands.Log += async (msg) =>
            {
                Console.WriteLine(msg);
#if DEBUG
                if (msg.Exception is CommandException ce)
                {
                    if (ce.InnerException is ArgumentException ae)
                    {
                        await ce.Context.Channel.SendMessageAsync(ae.Message);
                    }
                    else
                    {
                        await ce.Context.Channel.SendMessageAsync(embed: new EmbedBuilder
                        {
                            Color = Color.Red,
                            // We don't log raw FileNotFoundException message because they may contains local path
                            Title = msg.Exception.InnerException.GetType().ToString(),
                            Description = ce.InnerException is FileNotFoundException ? "Could not find file" : msg.Exception.InnerException.Message
                        }.Build());
                    }
                }
#endif
            };
        }

        private async Task MainAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            _commands.AddTypeReader<ColorString>(new ColorStringReader());

            await _commands.AddModuleAsync<ML>(null);
            await _commands.AddModuleAsync<Information>(null);

            var credentials = JsonSerializer.Deserialize<Credentials>(File.ReadAllText("Keys/Credentials.json"), new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            await _client.LoginAsync(TokenType.Bot, credentials.BotToken);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            if (arg is not SocketUserMessage msg || arg.Author.IsBot)
            {
                return;
            }
            int pos = 0;
            if (msg.HasMentionPrefix(_client.CurrentUser, ref pos) || msg.HasStringPrefix("r.", ref pos))
            {
                SocketCommandContext context = new(_client, msg);
                var result = await _commands.ExecuteAsync(context, pos, null);
                if (!result.IsSuccess)
                {
                    if (result.Error == CommandError.UnmetPrecondition || result.Error == CommandError.BadArgCount
                        || result.Error == CommandError.ParseFailed)
                    {
                        await msg.Channel.SendMessageAsync(result.ErrorReason);
                    }
                }
            }
        }
    }
}
