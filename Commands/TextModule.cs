using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Music_C_.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Commands
{
    public class TextModule : BaseCommandModule
    {
        private readonly WebCalService _webCal;
        private readonly ConfigService _config;

        public Random Rng { private get; set; }

        public TextModule(WebCalService webCal, ConfigService config)
        {
            _webCal = webCal;
            _config = config;
        }

        [Command("greet")]
        public async Task GreetCommand(CommandContext ctx)
        {
            await ctx.RespondAsync($"Greetings {ctx.Member.DisplayName}! Thanks for calling me!");
        }

        [Command("Random")]
        public async Task RandomNumber(CommandContext ctx, int min, int max)
        {
            await ctx.RespondAsync($"Your random number is: {Rng.Next(min, max)}");
        }

        [Command("Calender")]
        public async Task CalenderCommand(CommandContext ctx)
        {
            var calenders = await _webCal.GetCalenderEvents(_config.WebCalUrl);
            await ctx.RespondAsync(calenders);
        }
    }
}
