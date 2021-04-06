using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        ///     Command for getting info about a minecraft server
        /// </summary>
        /// <param name="context"></param>
        /// <param name="serverIp">The address of the server to be checked</param>
        /// <returns></returns>
        [Command("mcstatus")]
        [Description("Pings a Minecraft server")]
        public async Task ServerStatus(CommandContext context,
            [Description("The IP for the server you wish to ping")]
            string serverIp)
        {
            await context.TriggerTypingAsync();

            var key = CacheKeys.MinecraftServerKey + serverIp;

            _logger.LogInformation("Fetching bingus server status");
            var inCache = _cache.TryGetValue(key, out MinecraftStatus minecraftStatus);
            if (!inCache)
            {
                _logger.LogInformation("Contacting minecraft api");
                var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(30);
                var builder = new UriBuilder("https://mcapi.us/server/status");
                builder.Query = $"ip={serverIp}";
                var resp = await client.GetAsync(builder.Uri);
                if (resp.IsSuccessStatusCode)
                {
                    var content = await resp.Content.ReadAsStringAsync();
                    minecraftStatus = JsonConvert.DeserializeObject<MinecraftStatus>(content);
                    _cache.Set(key, minecraftStatus, TimeSpan.FromMinutes(1));
                }
                else
                {
                    _logger.LogError("Error {Status} occurred when contacting mcapi", resp.StatusCode.ToString());
                    await context.RespondAsync("An error occured, check the log.");
                    return;
                }
            }

            await SendMinecraftMessage(serverIp, minecraftStatus, context);
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
            await ServerStatus(context, "mc.dcurrey.co.uk");
        }

        private async Task SendMinecraftMessage(string serverIp,
            MinecraftStatus minecraftStatus,
            CommandContext context)
        {
            _logger.LogInformation("Sending minecraft server status message");
            var embedBuilder = new DiscordEmbedBuilder()
                .WithTitle("Minecraft Server Status")
                .WithDescription($"Fetched for {serverIp}")
                .WithFooter("Fetched using mcapi.us");

            if (minecraftStatus.Online)
            {
                embedBuilder.AddField("Status", "Online")
                    .AddField("Server Version", minecraftStatus.Server.Name)
                    .AddField("Players", $"{minecraftStatus.Players.Now} of {minecraftStatus.Players.Max}");
                if (minecraftStatus.Players.Sample.Count > 0)
                {
                    var players = minecraftStatus.Players.Sample.Aggregate
                        ("", (current, player) => current + $"{player.Name} ");
                    embedBuilder.AddField("Currently Online", players);
                }

                await context.RespondWithFileAsync(ConvertToPng(minecraftStatus.Favicon));
            }
            else
            {
                embedBuilder.AddField("Status", "Offline");
                embedBuilder.AddField("Error", minecraftStatus.Error);
            }

            await context.RespondAsync("", embed: embedBuilder.Build());
        }

        private string ConvertToPng(string base64String)
        {
            base64String = base64String.Substring(base64String.IndexOf(',') + 1);
            try
            {
                var imgBytes = Convert.FromBase64String(base64String);
                using (var imageFile = new FileStream(@"favicon.png", FileMode.Create))
                {
                    imageFile.Write(imgBytes, 0, imgBytes.Length);
                    imageFile.Flush();
                }

                return @"favicon.png";
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error ocurred");
                throw;
            }
        }
    }
}