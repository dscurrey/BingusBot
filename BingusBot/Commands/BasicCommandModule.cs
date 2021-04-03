using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;

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
            await context.RespondAsync($"BingusBot v{fileVersionInfo.FileVersion}");
        }

        [Command("interact")]
        [Description("Simple command to test interaction")]
        public async Task Interact(CommandContext context)
        {
            await context.TriggerTypingAsync();
            await context.RespondAsync("How are you?");

            var intr = context.Client.GetInteractivityModule();
            var reminderContent = await intr.WaitForMessageAsync
            (
                c => c.Author.Id == context.Message.Author.Id, TimeSpan.FromSeconds(30)
            );

            if (reminderContent == null)
            {
                await context.RespondAsync("No response.");
                return;
            }

            await context.RespondAsync("k then lol");
        }
    }
}