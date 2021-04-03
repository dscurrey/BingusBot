using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using BingusBot.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BingusBot
{
    /// <summary>
    /// Class containing the discord bot
    /// </summary>
    public class DiscordBot
    {
        private CancellationTokenSource CancellationTokenSource { get; set; }
        private IConfigurationRoot _config;
        private DiscordClient _discord;
        private CommandsNextModule _commands;
        private InteractivityModule _interactivity;
        private readonly ILogger<DiscordBot> _logger;
        
        /// <summary>
        /// Constructor for the discord bot
        /// </summary>
        /// <param name="logger">DI logger</param>
        public DiscordBot(ILogger<DiscordBot> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Starts the bot
        /// </summary>
        /// <param name="args">Any arguments passed</param>
        /// <returns>Task</returns>
        public async Task Run(string[] args) => await InitBot(args);

        /// <summary>
        /// Initialises and runs the bot.
        /// </summary>
        /// <param name="args">Any arguments passed</param>
        /// <returns>Task</returns>
        private async Task InitBot(string[] args)
        {
            try
            {
                CancellationTokenSource = new CancellationTokenSource();
                _logger.LogInformation("Bingus Bot loading...");
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                _logger.LogInformation($"Loaded BingusBot v{fileVersionInfo.FileVersion}");

                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", false, true)
                    .Build();
                _logger.LogInformation("Loaded config");
            
                _logger.LogInformation("Creating discord client");
                _discord = new DiscordClient
                (
                    new DiscordConfiguration
                    {
                        Token = _config.GetValue<string>("discord:token"),
                        TokenType = TokenType.Bot
                    }
                );

                _interactivity = _discord.UseInteractivity
                (
                    new InteractivityConfiguration
                    {
                        PaginationBehaviour = TimeoutBehaviour.Delete,
                        PaginationTimeout = TimeSpan.FromSeconds(30),
                        Timeout = TimeSpan.FromSeconds(30)
                    }
                );

                var deps = BuildDeps();

                _commands = _discord.UseCommandsNext
                (
                    new CommandsNextConfiguration
                    {
                        StringPrefix = _config.GetValue<string>("discord:CommandPrefix"),
                        Dependencies = deps
                    }
                );

                var type = typeof(IModule);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(s => s.GetTypes())
                    .Where(p => type.IsAssignableFrom(p) && !p.IsInterface);

                var typeList = types as Type[] ?? types.ToArray();
                foreach (var t in typeList)
                {
                    _commands.RegisterCommands(t);
                }
                _logger.LogInformation($"Loaded {typeList.Length} module(s)");

                RunAsync(args).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred creating the bot");
            }
        }

        /// <summary>
        /// Runs the bot asynchronously
        /// </summary>
        /// <param name="args">Any passed args</param>
        /// <returns>Task</returns>
        private async Task RunAsync(string[] args)
        {
            _logger.LogInformation("Connecting to discord");
            await _discord.ConnectAsync();
            _logger.LogInformation("Connection established with discord");

            while (!CancellationTokenSource.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1));
            }

            _logger.LogInformation("Disconnecting from discord");
            await _discord.DisconnectAsync();
        }

        private DependencyCollection BuildDeps()
        {
            using var deps = new DependencyCollectionBuilder();
            deps.AddInstance(_interactivity)
                .AddInstance(CancellationTokenSource)
                .AddInstance(_config)
                .AddInstance(_discord);

            return deps.Build();
        }
    }
}