using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Music_C_.Data;
using Music_C_.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Services
{

    public class MusicService
    {
        private readonly PlaylistDataAccess db;
        private readonly DiscordHelper helper;
        private readonly DiscordClient client;
        private readonly ConfigService config;
        private readonly LavalinkExtension lavalink;

        private LavalinkNodeConnection node;
        public LavalinkGuildConnection Player { get; private set; }

        public MusicService(PlaylistDataAccess db, DiscordHelper helper, DiscordClient client, ConfigService config)
        {
            this.db = db;
            this.helper = helper;
            this.client = client;
            this.config = config;
            lavalink = client.GetLavalink();
        }

        private string CreateNode()
        {
            if (node is null)
            {
                if (!lavalink.ConnectedNodes.Any())
                    return "There is a problem with your Lavalink connection!";

                node = lavalink.ConnectedNodes.Values.First();
            }
            return string.Empty;
        }

        private string CreatePlayer()
        {
            if (node.ConnectedGuilds.Count == 0)
            {
                return "There is a problem with your Lavalink connection!";
            }
            Player = node.GetGuildConnection(helper.DiscordGuild);

            Player.PlaybackFinished += Player_PlaybackFinished;

            return string.Empty;
        }

        public async Task<string> JoinChannel()
        {
            string output = CreateNode();

            if (!string.IsNullOrEmpty(output))
                return output;

            if (Player is not null && Player.IsConnected)
            {
                return string.Empty;
            }

            await node.ConnectAsync(helper.DiscordChannel);

            if (Player is null)
            {
                output = CreatePlayer();
                if (!string.IsNullOrEmpty(output))
                {
                    return output;
                }
            }
            output = $"Joined {helper.DiscordChannel.Name} channel!";
            return output;
        }

        public async Task<string> LeaveChannel()
        {
            string output = CreateNode();
            if (!string.IsNullOrEmpty(output))
                return output;

            if (Player is not null && Player.IsConnected)
            {
                await Player.DisconnectAsync();
                output = $"Left {helper.DiscordChannel.Name} channel!";
                Player = null;
            }

            return output;
        }

        private Task Player_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
