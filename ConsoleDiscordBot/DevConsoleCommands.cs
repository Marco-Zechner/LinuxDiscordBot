using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class DevConsoleCommands : ApplicationCommandModule
    {
        static readonly TextWriter defaultConsole = Console.Out;
        static StringWriterExt writer;
        public static HashSet<DiscordChannel> ConsoleChannels { get; set; } = [];


        [SlashCommand("DevConsole", "Send the Console output to the User")]
        public static async Task DevConsole(InteractionContext ctx,
            [Option("consoleChannel", "The Channel where to bot should seed console stuff")] DiscordChannel consoleChannel 
            )
        {
            await ctx.DeferAsync(true);

            if (consoleChannel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No Channel provided"));
                return;
            }

            if (!ConsoleChannels.Contains(consoleChannel))
            {
                ConsoleChannels.Add(consoleChannel);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {consoleChannel.Name} to the Console Channels"));
            }
            else
            {
                ConsoleChannels.Remove(consoleChannel);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed {consoleChannel.Name} from the Console Channels"));
            }
        }

        public static async Task SetupDevConsole()
        {
            writer = new StringWriterExt();
            writer.Flushed += Writer_Flushed;

            try
            {
                Console.SetOut(writer);
                await Program.DevConsoleRunning();
            }
            finally
            {
                Console.SetOut(defaultConsole);
            }
        }

        private static void Writer_Flushed(object sender, EventArgs e)
        {
            if (sender is StringWriterExt sw)
            {
                Console.SetOut(defaultConsole);
                string message = sw.ToString();
                Console.Write(message);
                OnConsoleWrite(message);
                sw.GetStringBuilder().Clear(); // Clear the buffer after writing to console
                Console.SetOut(writer);
            }
        }

        private static async Task OnConsoleWrite(string message)
        {
            message = message.TrimEnd('\n').Trim();

            if (string.IsNullOrEmpty(message))
            {
                return;
            }

            if (ConsoleChannels.Count == 0)
            {
                Console.WriteLine("\nNo Console Channels set");
                return;
            }

            foreach (var channel in ConsoleChannels)
            {
                string prefix = $"{DateTime.Now:[HH:mm:ss]}  ";

                //var previousMessage = channel.GetMessagesAsync(1);

                if (message.Length < 1900)
                    await channel.SendMessageAsync($"```\n{prefix}{message}```");
                else
                {
                    int i = 0;
                    while (i < message.Length)
                    {
                        await channel.SendMessageAsync($"```\n{prefix}{message.Substring(i, Math.Min(1900, message.Length - i))}```");
                        i += 1900;
                    }
                }
            }
        }
    }

    public class StringWriterExt : StringWriter
    {
        public event EventHandler Flushed;

        public StringWriterExt()
            : base() { }

        public override void Write(string value)
        {
            base.Write(value);
            Flush();
        }

        public override void Flush()
        {
            base.Flush();
            Flushed?.Invoke(this, EventArgs.Empty);
        }
    }
}
