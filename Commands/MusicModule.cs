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

        public MusicModule(MusicService music)
        {
            this.music = music;
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
            var respons = await music.LeaveChannel();

            if (!string.IsNullOrEmpty(respons))
                await ctx.RespondAsync(respons);
        }

        [Command("play")]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {

        }

        [Command("stop")]
        public async Task Stop(CommandContext ctx)
        {

        }

        [Command("resume")]
        public async Task Resume(CommandContext ctx)
        {

        }

        [Command("pause")]
        public async Task Pause(CommandContext ctx)
        {


        }

        [Command("skip")]
        public async Task SkipCurrentTrack(CommandContext ctx)
        {

        }

        [Command("replay")]
        public async Task ReplayPlaylist(CommandContext ctx)
        {

        }

        [Command("repeat")]
        public async Task DisableAutoReplay(CommandContext ctx)
        {

        }

        [Command("playlist")]
        public async Task GetPlaylist(CommandContext ctx)
        {

        }

        [Command("add")]
        public async Task AddTrack(CommandContext ctx, [RemainingText] string search)
        {

        }

        [Command("remove")]
        public async Task RemoveTrack(CommandContext ctx, [RemainingText] string search)
        {

        }

        [Command("playrandom")]
        public async Task PlayRadnom(CommandContext ctx)
        {

        }

        [Command("track")]
        public async Task CurrentTrack(CommandContext ctx)
        {

        }
    }
}
