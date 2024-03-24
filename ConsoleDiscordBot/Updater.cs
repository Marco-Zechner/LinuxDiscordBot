using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class Updater : ApplicationCommandModule
    {
        // major.minor.hotfix
        // major - a feature completed
        // minor - new features added
        // hotfix - bug fixes, small changes
        public const string Version = "1.0.0";

        [SlashCommand("BeamMeUpScotty", "Tries to update the Bot")]
        public static async Task UpdateBot(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Bot currently on Version {Version}. Bot will now attempt to restart and update."));
            
            Environment.Exit(1);
        }

        [SlashCommand("VersionInfo", "Prints the current Version of the Bot.")]
        public static async Task VersionInfo(InteractionContext ctx)
        {
            await ctx.DeferAsync(true);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Bot currently on Version {Version}."));
        }
    }
}
