using System.Reflection;

namespace ConsoleDiscordBot
{
    internal class Program
    {
        public static string ExeFolderPath { get; private set; }
        static async Task Main()
        {
            ExeFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            await DevConsoleCommands.SetupDevConsole();
        }

        public static async Task DevConsoleRunning()
        {
            await Bot.Setup();
        }
    }
}
