using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace BingusBot.Commands
{
    public class BasicCommandsModule : IModule
    {
        [Command("ping")]
        [Description("Test command to test if the bot is running")]
        public async Task Ping(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            await context.RespondAsync($"$BingusBot, v{fileVersionInfo.FileVersion}");
        }
    }
}