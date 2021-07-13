using System;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlusNextGen;
using DSharpPlusNextGen.CommandsNext;

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

            var commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new []{ "$" }
            });
            
            // Loads all commands
            commands.RegisterCommands(Assembly.GetExecutingAssembly());

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }
}