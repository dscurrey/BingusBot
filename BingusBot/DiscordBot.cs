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
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BingusBot
{
    /// <summary>
    ///     Class containing the discord bot
    /// </summary>
    public class DiscordBot
    {
        private readonly ILogger<DiscordBot> _logger;
        private readonly ServiceCollection _services = new();
        private CommandsNextModule _commands;
        private IConfigurationRoot _config;
        private DiscordClient _discord;
        private InteractivityModule _interactivity;

        /// <summary>
        ///     Constructor for the discord bot
        /// </summary>
        /// <param name="logger">DI logger</param>
        public DiscordBot(ILogger<DiscordBot> logger)
        {
            _logger = logger;
        }

        private CancellationTokenSource CancellationTokenSource { get; set; }

        /// <summary>
        ///     Starts the bot
        /// </summary>
        /// <param name="args">Any arguments passed</param>
        /// <returns>Task</returns>
        public async Task Run(string[] args)
        {
            await InitBot(args);
        }

        /// <summary>
        ///     Initialises and runs the bot.
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
                        Token = GetConfigToken(),
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

                ConfigureServices(_services);
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
                foreach (var t in typeList) _commands.RegisterCommands(t);
                _logger.LogInformation($"Loaded {typeList.Length} module(s)");

                RunAsync(args).Wait();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred creating the bot");
            }
        }

        /// <summary>
        ///     Runs the bot asynchronously
        /// </summary>
        /// <param name="args">Any passed args</param>
        /// <returns>Task</returns>
        private async Task RunAsync(string[] args)
        {
            _logger.LogInformation("Connecting to discord");
            await _discord.ConnectAsync();
            _logger.LogInformation("Connection established with discord");

            while (!CancellationTokenSource.IsCancellationRequested) await Task.Delay(-1);

            _logger.LogInformation("Disconnecting from discord");
            await _discord.DisconnectAsync();
        }

        private DependencyCollection BuildDeps()
        {
            using var deps = new DependencyCollectionBuilder();
            deps.AddInstance(_interactivity)
                .AddInstance(CancellationTokenSource)
                .AddInstance(_config)
                .AddInstance(_discord)
                .AddInstance(_services.BuildServiceProvider());
            return deps.Build();
        }

        private string GetConfigToken()
        {
            return Environment.GetEnvironmentVariable("DISCORD_TOKEN") ?? _config.GetValue<string>("discord:token");
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging
            (
                builder =>
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                    builder.AddNLog("nlog.config");
                }
            );
            services.AddMemoryCache();
        }
    }
}