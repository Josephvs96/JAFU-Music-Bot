using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Music_C_.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Commands
{
    class MusicModule : BaseCommandModule
    {
        private readonly MusicService music;
        private readonly ConfigService config;

        public MusicModule(MusicService music, ConfigService config)
        {
            this.music = music;
            this.config = config;
        }
        [Command("join")]
        public async Task Join(CommandContext ctx)
        {
            var respons = await music.JoinChannel();

            if (!string.IsNullOrEmpty(respons))
                await ctx.RespondAsync(respons);
        }

        [Command("leave")]
        public async Task Leave(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;

            var respons = await music.LeaveChannel();

            if (!string.IsNullOrEmpty(respons))
                await ctx.RespondAsync(respons);
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;

            var respons = await music.Play(search);
            await ctx.RespondAsync(respons);
        }

        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            await music.StopPlaying();
        }

        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            await music.ResumePlayback();
        }

        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            await music.PausePlayback();
        }

        [Command("skip")]
        public async Task SkipCurrentTrack(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            await music.SkipCurrentTrack();
        }

        [Command("replay")]
        public async Task ReplayPlaylist(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            await music.ReplayPlaylist();
        }

        [Command("repeat")]
        public async Task DisableAutoReplay(CommandContext ctx)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            await ctx.RespondAsync(music.ToggleAutoReplay());
        }

        [Command("playlist")]
        public async Task GetPlaylist(CommandContext ctx)
        {
            var playlist = await music.GetPlaylistTracks();
            await ctx.RespondAsync(playlist);
        }

        [Command("remove")]
        public async Task RemoveTrack(CommandContext ctx, [RemainingText] string search)
        {
            if (!await CheckMemberIsConnectedToVoiceChannel(ctx))
                return;
            var response = await music.RemoveFromPlaylist(search);
            await ctx.RespondAsync(response);
        }

        [Command("playrandom")]
        public async Task PlayRadnom(CommandContext ctx)
        {

        }

        [Command("track")]
        public async Task CurrentTrack(CommandContext ctx)
        {
            var trackInfo = music.GetCurrentTrackInfo();
            await ctx.RespondAsync(trackInfo);
        }

        private async Task<bool> CheckMemberIsConnectedToVoiceChannel(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null || ctx.Member.VoiceState.Channel.Name.ToLower() != config.Channel)
            {
                await ctx.RespondAsync($"You have to be in the music channel to use this command");
                return false;
            }
            return true;
        }
    }
}
