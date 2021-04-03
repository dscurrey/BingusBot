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
    public class DiscordBot
    {
        private CancellationTokenSource _cancellationTokenSource { get; set; }
        private IConfigurationRoot _config;
        private DiscordClient _discord;
        private CommandsNextModule _commands;
        private InteractivityModule _interactivity;
        private readonly ILogger<DiscordBot> _logger;
        public DiscordBot(ILogger<DiscordBot> logger)
        {
            _logger = logger;
        }
        
        public async Task Run(string[] args) => await InitBot(args);

        async Task InitBot(string[] args)
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _logger.LogInformation("Bingus Bot loading...");
                var assembly = Assembly.GetExecutingAssembly();
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                _logger.LogInformation($"Loaded BingusBot v{fileVersionInfo.FileVersion}");

                _config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
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

        async Task RunAsync(string[] args)
        {
            _logger.LogInformation("Connecting to discord");
            await _discord.ConnectAsync();
            _logger.LogInformation("Connection established with discord");

            while (!_cancellationTokenSource.IsCancellationRequested)
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
                .AddInstance(_cancellationTokenSource)
                .AddInstance(_config)
                .AddInstance(_discord);

            return deps.Build();
        }
    }
}