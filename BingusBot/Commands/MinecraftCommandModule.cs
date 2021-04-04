using System;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BingusBot.Commands
{
    /// <summary>
    ///     Module for Minecraft server commands
    /// </summary>
    public class MinecraftCommandModule : IModule
    {
        private static IMemoryCache _cache;
        private readonly ILogger<MinecraftCommandModule> _logger;

        /// <summary>
        ///     Constructor for Minecraft Command Module
        /// </summary>
        /// <param name="services">Service Provider</param>
        public MinecraftCommandModule(ServiceProvider services)
        {
            _cache = services.GetService<IMemoryCache>();
            _logger = services.GetService<ILogger<MinecraftCommandModule>>();
        }

        /// <summary>
        ///     Command for pinging BingusBois server, returning info
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [Command("bing")]
        [Description("Pings the bingus Minecraft server")]
        public async Task Bingus(CommandContext context)
        {
            await context.TriggerTypingAsync();

            var url = "mc.dcurrey.co.uk";
            var key = CacheKeys.MinecraftServerKey + url;
            MinecraftStatus minecraftStatus;

            _logger.LogInformation("Fetching bingus server status");
            var inCache = _cache.TryGetValue(key, out minecraftStatus);
            if (!inCache)
            {
                _logger.LogInformation("Contacting minecraft api");
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var builder = new UriBuilder("https://mcapi.us/server/status");
                builder.Query = $"ip={url}";
                var resp = await client.GetAsync(builder.Uri);
                var content = await resp.Content.ReadAsStringAsync();
                minecraftStatus = JsonConvert.DeserializeObject<MinecraftStatus>(content);
                _cache.Set(key, minecraftStatus, TimeSpan.FromMinutes(1));
            }

            await SendMinecraftMessage(minecraftStatus, context);
        }

        private async Task SendMinecraftMessage(MinecraftStatus minecraftStatus, CommandContext context)
        {
            var msgText = "";
            if (minecraftStatus.Online)
                msgText = "Bingus Bois is Online\n" +
                          $"Server: {minecraftStatus.Server.Name}\n" +
                          $"Players: {minecraftStatus.Players.Online} of {minecraftStatus.Players.Max}\n";
            foreach (var player in minecraftStatus.Players.Sample) msgText += $"{player.Name} ";

            if (msgText == "")
                await context.RespondAsync("Server could not be found");
            else
                await context.RespondAsync(msgText);
        }
    }
}