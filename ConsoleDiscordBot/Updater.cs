using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class Updater : ApplicationCommandModule
    {
        [SlashCommand("BeamMeUpScotty", "Tries to update the Bot")]
        public static async Task UpdateBot(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Bot will close now."));
            
            Environment.Exit(1);
        }
    }
}
