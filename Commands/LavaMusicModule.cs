using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Lavalink.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft;
using Music_C_.Data;
using Music_C_.Models;
using System.Text.RegularExpressions;
using Music_C_.Services;
using Music_C_.Helpers;

namespace Music_C_.Commands
{
    class LavaMusicModule : BaseCommandModule
    {
        private readonly ConfigService config;
        private readonly BotDbContext db;
        private readonly DiscordHelper helper;
        private readonly LavalinkExtension lavalink;
        private List<PlaylistTrackModel> playlist = new();
        private bool repeate = false;
        private bool isPlaying = false;
        private LavalinkGuildConnection player;

        public LavaMusicModule(ConfigService config, BotDbContext db, DiscordClient discord, DiscordHelper helper)
        {
            this.config = config;
            this.db = db;
            this.helper = helper;
            playlist.AddRange(db.Playlist);
            lavalink = discord.GetLavalink();
        }

        private void CreatePlayer(CommandContext ctx)
        {
            player = lavalink.GetGuildConnection(helper.DiscordGuild);
        }

        private string GetNextTrack()
        {
            var output = playlist.Where(x => x.IsPlayed != true).FirstOrDefault();

            if (output is null && repeate)
            {
                ResetPlaylist();
                output = playlist.Where(x => x.IsPlayed != true).FirstOrDefault();
            }

            if (output is null && repeate == false)
            {
                return "";
            }

            if (playlist.IndexOf(output) == 0)
            {
                ResetPlaylist();
            }

            output.IsPlayed = true;
            db.Playlist.Update(output);
            db.SaveChanges();
            return output.TrackName;
        }

        private void ResetPlaylist()
        {
            foreach (var item in db.Playlist)
            {
                item.IsPlayed = false;
            }
            db.SaveChanges();
            UpdatePlaylist();
        }

        private void UpdatePlaylist()
        {
            playlist.Clear();
            playlist.AddRange(db.Playlist);
        }

        [Command("replay")]
        public async Task ReplayPlaylist(CommandContext ctx)
        {
            isPlaying = false;
            ResetPlaylist();
            await Play(ctx, playlist[0].TrackName);
        }

        [Command("repeat")]
        public async Task DisableAutoReplay(CommandContext ctx)
        {
            repeate = !repeate;
            if (repeate)
                await ctx.RespondAsync("Repeat enabled!");
            else
                await ctx.RespondAsync("Repeat disabled!");
        }

        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            if (!lavalink.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink connection is not established");
                return;
            }
            var node = lavalink.ConnectedNodes.Values.First();
            var channel = ctx.Guild.Channels.Where(x => x.Value.Name.ToLower() == config.Channel).FirstOrDefault().Value;
            if (channel is null)
            {
                await ctx.RespondAsync("Cannot find a valid voice channel called Music!");
                return;
            }

            await node.ConnectAsync(channel);
            CreatePlayer(ctx);
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            var channel = ctx.Guild.Channels.Where(x => x.Value.Name.ToLower() == config.Channel).FirstOrDefault().Value;
            if (channel is null)
            {
                await ctx.RespondAsync("Cannot find a valid voice channel called Music!");
                return;
            }
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Lavalink connection is not established");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(helper.DiscordGuild);
            isPlaying = false;
            await conn.DisconnectAsync();
            player = null;
            await ctx.RespondAsync($"Left {helper.DiscordChannel.Name}!");
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config.Channel)
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

            if (playlist.Count == 0)
            {
                await AddTrack(ctx, search);
            }

            var loadResult = await node.Rest.GetTracksAsync(search);
            var track = loadResult.Tracks.First();
            if (playlist.FindAll(x => x.TrackName == track.Title).Count == 0)
            {
                await AddTrack(ctx, track.Title);
            }

            if (!isPlaying)
            {
                isPlaying = true;
                await TrackIsPlayed(track);
                await player.PlayAsync(track);
                await ctx.RespondAsync($"Now playing {track.Title}!");
                conn.PlaybackFinished += Conn_PlaybackFinished;
            }

        }

        private async Task TrackIsPlayed(LavalinkTrack track)
        {
            var playlistTrack = playlist.Find(x => x.TrackName == track.Title);
            playlistTrack.IsPlayed = true;
            db.Playlist.Update(playlistTrack);
            await db.SaveChangesAsync();
            UpdatePlaylist();
        }

        private async Task Conn_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            await Task.Delay(500);

            var nextTrack = GetNextTrack();
            if (string.IsNullOrEmpty(nextTrack))
            {
                return;
            }
            var loadResult = await sender.GetTracksAsync(nextTrack);
            var track = loadResult.Tracks.First();
            await sender.PlayAsync(track);
        }

        [Command("add")]
        public async Task AddTrack(CommandContext ctx, [RemainingText] string search)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config.Channel)
            {
                await ctx.RespondAsync($"You have to be in the music channel to use this command");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
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
            PlaylistTrackModel trackModel = new PlaylistTrackModel
            {
                TrackName = track.Title,
                TrackURL = track.Uri.AbsoluteUri,
                AddedDate = DateTime.Now,
                AddedBy = ctx.Member.DisplayName,
                IsPlayed = false
            };

            await db.AddAsync(trackModel);
            await db.SaveChangesAsync();

            await ctx.RespondAsync($"{trackModel.TrackName} add to the playlist!");
            UpdatePlaylist();
        }

        [Command("skip")]
        public async Task SkipCurrentTrack(CommandContext ctx)
        {
            var nextTrack = GetNextTrack();
            if (string.IsNullOrEmpty(nextTrack))
            {
                await ctx.RespondAsync("There are no more tracks in the playlist");
                await ctx.Client.GetLavalink().ConnectedNodes.First().Value.GetGuildConnection(ctx.Member.VoiceState.Guild).StopAsync();
            }
            else
            {
                isPlaying = false;
                await Stop(ctx);
                await Play(ctx, nextTrack);
            }
        }

        [Command("playlist")]
        public async Task GetPlaylist(CommandContext ctx)
        {
            UpdatePlaylist();
            string tracks = "";
            foreach (var item in playlist)
            {
                tracks += $"{item.TrackName}\n";
            }
            await ctx.RespondAsync(tracks);
        }

        [Command("playrandom")]
        public async Task PlayRadnom(CommandContext ctx)
        {
            var random = new Random();

            string[] searchKeywords = new[] { "Radnom music", "pop music", "rock music", "country music", "classical music" };
            string search = searchKeywords[random.Next(0, searchKeywords.Length - 1)];


            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config.Channel)
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
            isPlaying = false;
            var conn = await GetLavaLinkConnection(ctx);
            conn.PlaybackFinished -= Conn_PlaybackFinished;
            if (conn is not null)
                await conn.StopAsync();
        }


        //Helper method thar checks that the member is in a voice channel and the lavalink
        //connection is established as well as checking if there is a track playing

        private async Task<LavalinkGuildConnection> GetLavaLinkConnection(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config.Channel)
            {
                await ctx.RespondAsync($"You have to be in {config.Channel} channel in order to use this command");
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
