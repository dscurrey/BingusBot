using System;
using System.Threading.Tasks;
using DSharpPlus;

namespace BingusBot2
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var discord = new DiscordClient
            (
                new DiscordConfiguration
                {
                    Token = "",
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged
                }
            );
        }
    }
}