using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Newtonsoft;
using Newtonsoft.Json;

namespace Music_C_.Services
{
    public class ConfigService
    {
        private readonly string filePath = @"./settings.json";
        private readonly JsonSettings jsonSettings;

        public string Token { get; private set; }
        public ulong Guild { get; private set; }
        public string Channel { get; private set; }
        public string Prefix { get; private set; }
        public LavalinkConfiguration LavaConfig { get; private set; }
        public string WebCalUrl { get; private set; }
        public ConfigService()
        {
            jsonSettings = DeserialzieJsonFile();
            LavaConfig = CreateLavalinkConfig();
            Token = jsonSettings.Token;
            Prefix = jsonSettings.Prefix;
            Guild = jsonSettings.Guild;
            Channel = jsonSettings.Channel;
            WebCalUrl = jsonSettings.WebCalUrl;
        }

        private LavalinkConfiguration CreateLavalinkConfig()
        {
            var endpoint = new ConnectionEndpoint
            {
                Hostname = jsonSettings.LavaHost,
                Port = jsonSettings.LavaPort
            };

            var lavalinkConfig = new LavalinkConfiguration
            {
                Password = jsonSettings.LavaPassword,
                RestEndpoint = endpoint,
                SocketEndpoint = endpoint
            };

            return lavalinkConfig;
        }

        private JsonSettings DeserialzieJsonFile()
        {
            var serializer = new JsonSerializer();
            using (var sw = new StreamReader(filePath))
            using (var reader = new JsonTextReader(sw))
            {
                return serializer.Deserialize<JsonSettings>(reader);
            }
        }

    }

    public class JsonSettings
    {
        public string Token { get; private set; }

        public ulong Guild { get; private set; }

        public string Channel { get; private set; }

        public string Prefix { get; private set; }

        public string LavaPassword { get; private set; }

        public string LavaHost { get; private set; }

        public int LavaPort { get; private set; }

        public string WebCalUrl { get; private set; }

        public JsonSettings()
        {

        }

        [JsonConstructor]
        public JsonSettings(string token, ulong guild, string channel, string prefix, string lavaPassword, string lavaHost, int lavaPort, string webCalUrl)
        {
            this.Token = token;
            this.Guild = guild;
            this.Channel = channel;
            this.Prefix = prefix;
            this.LavaPassword = lavaPassword;
            this.LavaHost = lavaHost;
            this.LavaPort = lavaPort;
            this.WebCalUrl = webCalUrl;
        }
    }

}
