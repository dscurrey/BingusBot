using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;

namespace BingusBot.Commands
{
    /// <summary>
    ///     Commands module for basic (test) commands
    /// </summary>
    public class BasicCommandsModule : IModule
    {
        private IConfigurationRoot _config;

        /// <summary>
        ///     Constructor for Basic Commands module with injected services
        /// </summary>
        /// <param name="config"></param>
        public BasicCommandsModule(IConfigurationRoot config)
        {
            _config = config;
        }

        /// <summary>
        ///     Ping command for testing basic functions
        /// </summary>
        /// <param name="context">Command context</param>
        /// <returns>Task</returns>
        [Command("ping")]
        [Description("Test command to test if the bot is running")]
        public async Task Ping(CommandContext context)
        {
            await context.TriggerTypingAsync();
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            await context.RespondAsync($"BingusBot v{fileVersionInfo.FileVersion}");
        }

        /// <summary>
        ///     Interaction command for testing basic interactivity
        /// </summary>
        /// <param name="context">Context</param>
        /// <returns>Task</returns>
        [Command("interact")]
        [Description("Simple command to test interaction")]
        public async Task Interact(CommandContext context)
        {
            await context.TriggerTypingAsync();
            await context.RespondAsync("How are you?");

            var interactive = context.Client.GetInteractivityModule();
            var reminderContent = await interactive.WaitForMessageAsync
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