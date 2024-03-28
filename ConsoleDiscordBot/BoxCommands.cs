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
                DevConsoleCommands.CommandFailed(ctx.User, "BoxIt", new (string, string, string)[]
                {
                    ("header.Length", header.Length.ToString(), "200"),
                    ("content.Length", content.Length.ToString(), "800")
                }, "header or content was too long.");

                header = "Error";
                content = "Header or Content was too long.";
            }

            string boxedMessage = CodeBoxDrawer.DrawBoxWithHeader(header, content, (float)headerAlignment, 
                thickHeader: boxFormating == BoxFormating.thickHeader || boxFormating == BoxFormating.allThick,
                thickContent: boxFormating == BoxFormating.thickContent || boxFormating == BoxFormating.allThick);

            if (boxedMessage.Split('\n')[0].Length > 60)
            {
                try
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                        .WithContent($"```{boxedMessage}```")
                        );
                }
                catch (Exception ex)
                {
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                    .WithContent($"Invalid Text\n```{ex.Message}```")
                    );
                    return;
                }
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
