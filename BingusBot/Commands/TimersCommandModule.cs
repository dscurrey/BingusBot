using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;

namespace BingusBot.Commands
{
    public class TimersCommandModule : IModule
    {
        private readonly ILogger<TimersCommandModule> _logger;
        
        public TimersCommandModule(ServiceProvider services)
        {
            _logger = services.GetService<ILogger<TimersCommandModule>>();
        }

        [Command("timertest")]
        public async Task TimerTest(CommandContext context)
        {
            await context.Channel.SendMessageAsync("Working");
        }
    }
}