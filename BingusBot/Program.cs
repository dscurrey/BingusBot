using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace BingusBot
{
    static class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            ConfigureServices(services);

            await using (var serviceProvider = services.BuildServiceProvider())
            {
                var bot = serviceProvider.GetService<DiscordBot>();
                if (bot != null) await bot.Run(args);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
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

            services.AddMemoryCache();
        }
    }
}