using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Commands
{
    class LavaMusicModule : BaseCommandModule
    {
        private readonly IConfiguration config;

        public LavaMusicModule(IConfiguration config)
        {
            this.config = config;
        }

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink connection is not established");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var channel = ctx.Guild.Channels.Where(x => x.Value.Name.ToLower() == config["channel"]).FirstOrDefault().Value;
            if (channel is null)
            {
                await ctx.RespondAsync("Cannot find a valid voice channel called Music!");
                return;
            }

            await node.ConnectAsync(channel);
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx, DiscordChannel channel)
        {
            var conn = await GetLavaLinkConnection(ctx);
            if (conn is not null)
            {
                await conn.DisconnectAsync();
                await ctx.RespondAsync($"Left {channel.Name}!");
            }
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config["channel"])
            {
                await ctx.RespondAsync($"You have to be in the music channel to use this command");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await Join(ctx);
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            //We don't need to specify the search type here
            //since it is YouTube by default.
            var loadResult = await node.Rest.GetTracksAsync(search);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }

        [Command("playrandom")]
        public async Task PlayRadnom(CommandContext ctx)
        {
            var random = new Random();

            string[] searchKeywords = new[] { "Radnom music", "pop music", "rock music", "country music", "classical music" };
            string search = searchKeywords[random.Next(0, searchKeywords.Length - 1)];


            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config["channel"])
            {
                await ctx.RespondAsync($"You have to be in the music channel to use this command");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await Join(ctx);
                conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);
            }

            //We don't need to specify the search type here
            //since it is YouTube by default.
            var loadResult = await node.Rest.GetTracksAsync(search);

            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }


            var tracklist = loadResult.Tracks.ToList();
            var track = tracklist[random.Next(0, tracklist.Count - 1)];

            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }

        [Command("track")]
        public async Task CurrentTrack(CommandContext ctx)
        {
            var conn = await GetLavaLinkConnection(ctx);
            if (conn is not null)
            {
                var track = conn.CurrentState.CurrentTrack;
                await ctx.RespondAsync($"Title: {track.Title}\n" +
                    $"Author: {track.Author}\n" +
                    $"Length: {track.Length}\n" +
                    $"Time left: {track.Length - conn.CurrentState.PlaybackPosition:hh\\:mm\\:ss}");
            }
        }

        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            var conn = await GetLavaLinkConnection(ctx);
            if (conn is not null)
                await conn.PauseAsync();

        }

        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {
            var conn = await GetLavaLinkConnection(ctx);
            if (conn is not null)
                await conn.ResumeAsync();

        }

        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            var conn = await GetLavaLinkConnection(ctx);
            if (conn is not null)
                await conn.StopAsync();

        }


        //Helper method thar checks that the member is in a voice channel and the lavalink
        //connection is established as well as checking if there is a track playing

        private async Task<LavalinkGuildConnection> GetLavaLinkConnection(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config["channel"])
            {
                await ctx.RespondAsync($"You have to be in {config["channel"]} channel in order to use this command");
                return null;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return null;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks playing.");
                return null;
            }

            return conn;
        }

    }
}
