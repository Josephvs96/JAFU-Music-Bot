using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.Entities;
using DSharpPlus.Lavalink.EventArgs;
using DSharpPlus.Exceptions;
using Music_C_.Data;
using Music_C_.Helpers;
using Music_C_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Music_C_.Exceptions;

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
        private bool isPlaying;
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
            if (Player is null || !Player.IsConnected)
            {
                await JoinChannel();
            }

            var loadResult = await node.Rest.GetTracksAsync(search);

            var track = loadResult.Tracks.First();
            if (track is null)
            {
                return $"Can't find any resaults for {search}";
            }

            bool isPlaylistEmpty = playlist.Count == 0;
            bool isPlaylistContainTrack = playlist.FindAll(x => x.TrackName == track.Title).Count == 0;
            if (isPlaylistEmpty || isPlaylistContainTrack)
            {
                await AddTrackToPlayList(track);
            }

            if (!isPlaying)
            {
                isPlaying = true;
                await TrackIsPlayed(track);
                await Player.PlayAsync(track);
                return ($"Now playing {track.Title}!");
            }
            else
            {
                //TODO: Move the requested track to the end of the playlist
            }
            return string.Empty;
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

        private async Task AddTrackToPlayList(LavalinkTrack track)
        {
            var playlistTrack = TrackConverter.ConvertToPlaylistTrack(track);
            await db.AddTrack(playlistTrack);
        }

        private Task Player_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
