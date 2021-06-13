using DSharpPlus;
using DSharpPlus.Entities;
using Music_C_.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Helpers
{
    public class DiscordHelper
    {
        private readonly ConfigService config;

        public DiscordGuild DiscordGuild { get; private set; }
        public DiscordChannel DiscordChannel { get; private set; }
        public DiscordHelper(ConfigService config)
        {
            this.config = config;
        }

        public async Task GetGuild(DiscordClient client)
        {
            if (client != null && client.Guilds.Count != 0)
                DiscordGuild = await client.GetGuildAsync(config.Guild, true);
            GetChannel();
        }

        private void GetChannel()
        {
            if (DiscordGuild != null && DiscordGuild.Channels.Count != 0)
                DiscordChannel = DiscordGuild.Channels.Values.Where(x => x.Name.ToLower() == config.Channel).First();
        }

        public DiscordActivity GetActivity(string message) => new DiscordActivity(message);

    }
}
