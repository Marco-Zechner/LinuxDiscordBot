using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class BoxCommands : ApplicationCommandModule
    {
        public enum BoxFormating
        {
            [ChoiceName("═Header ─Content")]
            thickHeader,
            [ChoiceName("─Header ═Content")]
            thickContent,
            [ChoiceName("─Header ─Content")]
            allThin,
            [ChoiceName("═Header ═Content")]
            allThick,
        }

        [SlashCommand("BoxIt", "Boxes your message")]
        public async Task BoxIt(InteractionContext ctx,
            [Option("header", "Header of the Box")] string header,
            [Option("content", "Content of the Box")] string content,
            [Option("boxFormatting", "Use single or double Lines")] BoxFormating boxFormating = BoxFormating.thickHeader,
            [Option("headerAlignment", "Align the header from the left")] double headerAlignment = 0.5
            )
        {

            await ctx.DeferAsync();

            if (header.Length > 200 || content.Length > 800)
            {
                header = "No";
                content = "Mona, NO!";
            }

            string boxedMessage = CodeBoxDrawer.DrawBoxWithHeader(header, content, (float)headerAlignment, 
                thickHeader: boxFormating == BoxFormating.thickHeader || boxFormating == BoxFormating.allThick,
                thickContent: boxFormating == BoxFormating.thickContent || boxFormating == BoxFormating.allThick);

            if (boxedMessage.Split('\n')[0].Length > 60)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"```{boxedMessage}```")
                    );
                return;
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
            .AddEmbed(new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = ctx.User.Username,
                    IconUrl = ctx.User.AvatarUrl
                },
                Description = $"```{boxedMessage}```"
            })
            );

        }

    }
}
