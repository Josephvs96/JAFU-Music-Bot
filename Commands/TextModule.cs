using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Music_C_.Commands
{
    public class TextModule : BaseCommandModule
    {
        [Command("greet")]
        public async Task GreetCommand(CommandContext ctx)
        {
            await ctx.RespondAsync($"Greetings {ctx.Member.DisplayName}! Thanks for calling me!");
        }

        public Random Rng { private get; set; }

        [Command("Random")]
        public async Task RandomNumber(CommandContext ctx, int min, int max)
        {
            await ctx.RespondAsync($"Your random number is: {Rng.Next(min, max)}");
        }

    }
}
