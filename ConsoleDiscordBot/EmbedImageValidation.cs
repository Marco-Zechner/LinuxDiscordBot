using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class EmbedImageValidation
    {
        public static async Task<bool> Validate(InteractionContext ctx, DiscordAttachment? background = null)
        {
            if (background != null && background.MediaType?.Split('/')[0] != "image")
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Invalid image \"{background.FileName}\" is not an Image (image is optional)")
                );
                return false;
            }
            return true;
        }
    }
}
