using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace BingusBot
{
    public class DiscordBot
    {
        private readonly ILogger<DiscordBot> _logger;
        public DiscordBot(ILogger<DiscordBot> logger)
        {
            _logger = logger;
        }
        public async Task Run(string[] args) => await InitBot(args);

        async Task InitBot(string[] args)
        {
            _logger.LogInformation("Hello world");
        }
    }
}