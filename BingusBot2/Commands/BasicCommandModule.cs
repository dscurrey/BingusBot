using System.Threading.Tasks;
using DSharpPlusNextGen.CommandsNext;
using DSharpPlusNextGen.CommandsNext.Attributes;

namespace BingusBot2.Commands
{
    public class BasicCommandModule : BaseCommandModule
    {
        [Command("hello")]
        public async Task GreetCommand(CommandContext context)
        {
            await context.RespondAsync("I am alive.");
        }
    }
}