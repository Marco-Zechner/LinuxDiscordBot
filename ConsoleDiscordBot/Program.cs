using DiscordBot;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Interactivity;

namespace ConsoleDiscordBot
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await Bot.Setup();
        }
    }
}
