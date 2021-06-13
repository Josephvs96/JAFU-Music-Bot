using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Music_C_.Data;
using Music_C_.Exceptions;
using Music_C_.Helpers;
using Music_C_.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Music_C_.Services
{

    public class MusicService
    {
        #region Fields And Props  
        private readonly PlaylistDataAccess db;
        private readonly DiscordHelper helper;
        private readonly DiscordClient client;
        private readonly ConfigService config;
        private readonly LavalinkExtension lavalink;

        private List<PlaylistTrackModel> playlist;
        private bool isPlaying = false;
        private bool repeate = false;
        private LavalinkNodeConnection node;
        public LavalinkGuildConnection Player { get; private set; }
        #endregion
        public MusicService(PlaylistDataAccess db, DiscordHelper helper, DiscordClient client, ConfigService config)
        {
            this.db = db;
            this.helper = helper;
            this.client = client;
            this.config = config;
            lavalink = client.GetLavalink();
            playlist = db.GetAllPlaylistTracks().Result;
        }

        private void CreateNode()
        {
            if (node is not null)
            {
                return;
            }

            if (!lavalink.ConnectedNodes.Any())
                throw new NodeNotConnectedException("There is a problem with your Lavalink connection!");

            node = lavalink.ConnectedNodes.Values.First();
        }

        private void CreatePlayer()
        {
            if (node is null || node.ConnectedGuilds.Count == 0)
            {
                throw new NodeNotConnectedException("There is a problem with your Lavalink connection!");
            }

            Player = node.GetGuildConnection(helper.DiscordGuild);

            Player.PlaybackFinished += Player_PlaybackFinished;
        }

        public async Task<string> JoinChannel()
        {
            try
            {
                CreateNode();

                if (Player is not null && Player.IsConnected)
                {
                    return string.Empty;
                }

                await node.ConnectAsync(helper.DiscordChannel);

                if (Player is null)
                {
                    CreatePlayer();
                }

                var output = $"Joined {helper.DiscordChannel.Name} channel!";
                return output;
            }
            catch (NodeNotConnectedException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> LeaveChannel()
        {
            try
            {
                CreateNode();

                if (Player is not null && Player.IsConnected)
                {
                    await Player.DisconnectAsync(true);
                }

                var output = $"Left {helper.DiscordChannel.Name} channel!";
                Player = null;
                return output;
            }
            catch (NodeNotConnectedException ex)
            {
                return ex.Message;
            }
        }

        public async Task<string> Play(string search)
        {
            string output = "";
            if (Player is null || !Player.IsConnected)
            {
                await JoinChannel();
            }

            LavalinkTrack track = await GetLavaLinkTrack(search);

            if (track is null)
            {
                return $"Can't find any resaults for {search}";
            }

            bool isPlaylistEmpty = playlist.Count == 0;
            bool isPlaylistContainTrack = playlist.FindAll(x => x.TrackName == track.Title).Count == 0;
            if (isPlaylistEmpty || isPlaylistContainTrack)
            {
                output += await AddTrackToPlayList(track) + "\n";
            }

            if (!isPlaying)
            {
                isPlaying = true;
                await PlayTrack(track);
            }
            else
            {
                //TODO: Move the requested track to the end of the playlist
            }
            return output;
        }

        private async Task PlayTrack(LavalinkTrack trackToPlay)
        {
            await Player.PlayAsync(trackToPlay);
            await TrackIsPlayed(trackToPlay);
            await client.UpdateStatusAsync(helper.GetActivity(trackToPlay.Title));
        }

        public async Task StopPlaying()
        {
            isPlaying = false;
            Player.PlaybackFinished -= Player_PlaybackFinished;
            if (Player is not null)
                await Player.StopAsync();
        }

        public async Task PausePlayback()
        {
            if (Player is not null)
            {
                await Player.PauseAsync();
            }
        }

        public async Task ResumePlayback()
        {
            if (Player is not null)
            {
                await Player.ResumeAsync();
            }
        }

        public async Task SkipCurrentTrack()
        {
            if (Player is not null && Player.CurrentState.CurrentTrack is not null)
            {
                await Player.SeekAsync(Player.CurrentState.CurrentTrack.Length);
            }
        }

        public async Task ReplayPlaylist()
        {
            if (Player is null)
            {
                await JoinChannel();
            }
            await SkipCurrentTrack();
            await ResetPlaylist();
            var track = await GetNextTrackToPlay();
            await PlayTrack(track);
        }

        public string ToggleAutoReplay()
        {
            repeate = !repeate;
            if (repeate)
                return "Repeat enabled!";
            else
                return "Repeat disabled!";
        }

        public string GetCurrentTrackInfo()
        {
            if (Player is not null && Player.CurrentState.CurrentTrack is not null)
            {
                var track = Player.CurrentState.CurrentTrack;
                return $"Title: {track.Title}\n" +
                    $"Author: {track.Author}\n" +
                    $"Length: {track.Length}\n" +
                    $"Time left: {track.Length - Player.CurrentState.PlaybackPosition:hh\\:mm\\:ss}";
            }
            else return "Currently not playing music.";
        }

        public async Task<string> GetPlaylistTracks()
        {
            await UpdatePlaylist();
            string tracks = "";
            foreach (var item in playlist)
            {
                tracks += $"{item.TrackName}\n";
            }
            if (string.IsNullOrEmpty(tracks))
            {
                return "The playlist is empty";
            }
            return tracks;
        }

        public async Task<string> RemoveFromPlaylist(string search)
        {
            if (Player is null)
                await JoinChannel();

            if (playlist.Count <= 0)
            {
                return "The playlist doesn't conatin any tracks to be removed";
            }

            var lavalinkTrack = await GetLavaLinkTrack(search);
            if (lavalinkTrack is not null)
            {
                var playlistTrack = playlist.Where(x => x.TrackName == lavalinkTrack.Title).FirstOrDefault();
                if (playlistTrack is not null)
                {
                    await db.RemoveTrack(playlistTrack);
                    return $"{playlistTrack.TrackName} removed form the playlist!";
                }
            }
            return "Can't find the required track to remove!";

        }

        private async Task<LavalinkTrack> GetNextTrackToPlay()
        {
            var trackFromPlaylist = playlist.Where(x => x.IsPlayed != true).FirstOrDefault();

            if (trackFromPlaylist is null && repeate)
            {
                await ResetPlaylist();
                trackFromPlaylist = playlist.Where(x => x.IsPlayed != true).FirstOrDefault();
            }

            if (trackFromPlaylist is null && repeate == false)
            {
                return null;
            }

            if (playlist.IndexOf(trackFromPlaylist) == 0)
            {
                await ResetPlaylist();
            }

            var trackResults = await node.Rest.GetTracksAsync(trackFromPlaylist.TrackURL);
            var trackToBePlayed = trackResults.Tracks.First();

            return trackToBePlayed;
        }

        private async Task ResetPlaylist()
        {
            foreach (var track in playlist)
            {
                await db.UpdateToNotPlayedStatus(track);
            }
            await UpdatePlaylist();
        }

        private async Task UpdatePlaylist()
        {
            playlist.Clear();
            playlist = await db.GetAllPlaylistTracks();
        }

        private async Task TrackIsPlayed(LavalinkTrack track)
        {
            var playlistTrack = TrackConverter.ConvertToPlaylistTrack(track);
            await db.UpdateToPlayedStatus(playlistTrack);
        }

        private async Task<string> AddTrackToPlayList(LavalinkTrack track)
        {
            var playlistTrack = TrackConverter.ConvertToPlaylistTrack(track);
            await db.AddTrack(playlistTrack);
            return $"Added {track.Title} to the playlist";
        }

        private async Task<LavalinkTrack> GetLavaLinkTrack(string search)
        {
            var loadResult = await node.Rest.GetTracksAsync(search);
            var track = loadResult.Tracks.First();
            return track;
        }

        private async Task Player_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            await Task.Delay(500);

            var trackToPlay = await GetNextTrackToPlay();
            if (trackToPlay is not null)
            {
                await PlayTrack(trackToPlay);
            }
            else
            {
                await client.UpdateStatusAsync(helper.GetActivity($"Nothing, Just chilling 😁"));
            }
        }
    }
}
