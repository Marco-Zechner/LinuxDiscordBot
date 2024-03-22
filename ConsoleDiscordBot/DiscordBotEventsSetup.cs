using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class DiscordBotEventsSetup
    {
        public static void BeforeConnect(SlashCommandsExtension slashCommands)
        {
            Bot.Client.ComponentInteractionCreated += InteractionsHandler.Client_ComponentInteractionCreated;

            slashCommands.RegisterCommands<EventScheduling>();
        }
    }
}
