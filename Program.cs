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
using Music_C_.Services;
using System.Linq;

namespace Music_C_
{
    class Program
    {
        public static ServiceProvider Services { get; private set; }

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {

            ConfigService configService = new();

            Services = new ServiceCollection()
                .AddSingleton<Random>()
                .AddSingleton(configService)
                .AddDbContext<BotDbContext>()
                .AddSingleton<PlaylistDataAccess>()
                .AddSingleton<DiscordClient>()
                .AddScoped<WebCalService>()
                .AddSingleton(new DiscordConfiguration()
                {
                    Token = configService.Token,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged,
                })
                .AddSingleton<DiscordHelper>()
                .AddSingleton<MusicService>()
                .BuildServiceProvider();

            var discord = Services.GetRequiredService<DiscordClient>();

            var lavalink = discord.UseLavalink();

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration()
            {
                StringPrefixes = new[] { configService.Prefix },
                Services = Services
            });

            commands.RegisterCommands<TextModule>();
            commands.RegisterCommands<MusicModule>();

            await discord.ConnectAsync();
            await lavalink.ConnectAsync(configService.LavaConfig);

            discord.Ready += Discord_Ready;

            await Task.Delay(-1);
        }

        private static async Task Discord_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            var discordHelper = Services.GetRequiredService<DiscordHelper>();
            await discordHelper.GetGuild(sender);
        }
    }
}
