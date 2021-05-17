using System;
using System.Threading.Tasks;
using DSharpPlus;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.IO;
using DSharpPlus.Entities;
using DSharpPlus.CommandsNext;
using Music_C_.Commands;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.VoiceNext;
using DSharpPlus.Net;
using DSharpPlus.Lavalink;
using Music_C_.Data;

namespace Music_C_
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var builder = new ConfigurationBuilder()
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("settings.json");

            var configuration = builder.Build();

            var services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton<IConfiguration>(configuration)
                .AddDbContext<PlaylistContext>()
                .BuildServiceProvider();

            var discord = new DiscordClient(new DiscordConfiguration()
            {
                Token = configuration["token"],
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged,
            });

            var endpoint = new ConnectionEndpoint
            {
                Hostname = "127.0.0.1", // From your server configuration.
                Port = 2333 // From your server configuration
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = "youshallnotpass", // From your server configuration.
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            discord.UseVoiceNext();
            var lavalink = discord.UseLavalink();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { "!" },
                Services = services
            });

            commands.RegisterCommands<TextModule>();
            commands.RegisterCommands<LavaMusicModule>();

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(lavalinkConfig);
            await Task.Delay(-1);
        }
    }
}
