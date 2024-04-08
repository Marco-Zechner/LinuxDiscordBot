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
            [Option("content", "Content of the Box")] string content,
            [Option("header", "Header of the Box")] string header = "",
            [Option("boxFormatting", "Use single or double Lines")] BoxFormating boxFormating = BoxFormating.thickHeader,
            [Option("headerAlignment", "Align the header from the left")] double headerAlignment = 0.5
            )
        {
            await ctx.DeferAsync();

            if (header.Length > 200)
            {
                DevConsoleCommands.CommandFailed(ctx.User, "BoxIt", new (string, string, string)[]
                {
                    ("header.Length", header.Length.ToString(), "<=200"),
                }, "header was too long. [200 max]");

                header = "Error";
                content = "Header was too long.";
            }

            if (content.Length > 800)
            {
                DevConsoleCommands.CommandFailed(ctx.User, "BoxIt", new (string, string, string)[]
                {
                    ("content.Length", content.Length.ToString(), "<=800")
                }, "content was too long.");

                header = "Error";
                content = "Content was too long. [800 max, forced 200 per line]";
            }

            string[] contentLines = content.Split('\n');
            content = "";
            foreach (var contentLine in contentLines)
            {
                string line = contentLine;
                while (line.Length > 200)
                {
                    content += line[..200] + "\n";
                    line = line[201..];
                }
                content += line + "\n";
            }

            string boxedMessage;
            if (string.IsNullOrEmpty(header) == false)
            {
                boxedMessage = CodeBoxDrawer.DrawBoxWithHeader(header, content, (float)headerAlignment,
                thickHeader: boxFormating == BoxFormating.thickHeader || boxFormating == BoxFormating.allThick,
                thickContent: boxFormating == BoxFormating.thickContent || boxFormating == BoxFormating.allThick);
            }
            else
            {
                boxedMessage = CodeBoxDrawer.DrawBox(content, 
                    thick: boxFormating == BoxFormating.allThick || boxFormating == BoxFormating.thickContent);
            }

            if (boxedMessage.Split('\n')[0].Length > 40)
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
