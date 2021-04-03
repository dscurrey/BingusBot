using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BingusBot
{
    class Program
    {
        /// <summary>
        /// Cancellation token to end bot where required
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource { get; set; }
        
        /// <summary>
        /// Contains app config
        /// </summary>
        private IConfigurationRoot _config;
        
        /// <summary>
        /// Discord client
        /// </summary>
        private DiscordClient _discord;
        
        /// <summary>
        /// Module for discord commands
        /// </summary>
        private CommandsNextModule _commands;
        
        /// <summary>
        /// Module for discord interactivity
        /// </summary>
        private InteractivityModule _interactivity;

        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            using (ServiceProvider serviceProvider = services.BuildServiceProvider())
            {
                DiscordBot bot = serviceProvider.GetService<DiscordBot>();
                if (bot != null) await bot.Run(args);
            }
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<DiscordBot>();
            services.AddLogging
            (
                builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                    builder.AddNLog("nlog.config");
                }
            );
            
            // TODO: Add Caching
        }
    }
}