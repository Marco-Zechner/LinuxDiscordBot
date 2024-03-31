using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System.Timers;
using Timer = System.Timers.Timer;

namespace ConsoleDiscordBot
{
    public class DevConsoleCommands : ApplicationCommandModule
    {
        static readonly TextWriter defaultConsole = Console.Out;
        static StringWriterExt writer;
        public static HashSet<DiscordChannel> ConsoleChannels { get; set; } = [];

        private static Timer timer;
        private static readonly object lockObject = new object();


        [SlashCommand("DevConsole", "Send the Console output to the User")]
        public static async Task DevConsole(InteractionContext ctx,
            [Option("consoleChannel", "The Channel where to bot should seed console stuff")] DiscordChannel consoleChannel 
            )
        {
            await ctx.DeferAsync();

            if (consoleChannel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No Channel provided"));

                CommandFailed(ctx.User, "DevConsole", new (string, string, string)[]
                {
                    ("Channel", "null", "channelName"),
                }, "No Channel provided");

                return;
            }

            if (!ConsoleChannels.Contains(consoleChannel))
            {
                ConsoleChannels.Add(consoleChannel);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Added {consoleChannel.Name} to the Console Channels"));
                
                if (!string.IsNullOrEmpty(combinedMessage))
                {
                    await Task.Run(WriteToConsole);
                }
            }
            else
            {
                ConsoleChannels.Remove(consoleChannel);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Removed {consoleChannel.Name} from the Console Channels"));
            }

            File.WriteAllText($"{Program.ExeFolderPath}/consoleChannels.json", JsonConvert.SerializeObject(ConsoleChannels.Select(c => c.Id).ToArray()));
        }

        [SlashCommand("ListDevConsols", "List all Console Channels")]
        public static async Task ListDevConsols(InteractionContext ctx,
            [Option("showOtherServers", "Show the Console Channels of other Servers")] bool showOtherServers = false
            )
        {
            await ctx.DeferAsync();

            if (ConsoleChannels.Count == 0)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No Console Channels set"));
                return;
            }

            string message = "Console Channels:\n";
            foreach (var channel in ConsoleChannels)
            {
                if (!showOtherServers && channel.GuildId != ctx.Guild.Id)
                    continue;
                message += $"- {channel.Name} : {channel.Guild.Name}\n";
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(message));
        }

        [SlashCommand("WriteToConsole", "Write a Message to the Console")]
        public static async Task WriteToConsole(InteractionContext ctx,
            [Option("message", "The Message to write to the Console")] string message
            )
        {
            await ctx.DeferAsync();

            if (string.IsNullOrEmpty(message))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No Message provided"));

                CommandFailed(ctx.User, "WriteToConsole", new (string, string, string)[]
                {
                    ("Message", "null", "message"),
                }, "No Message provided");

                return;
            }

            Console.WriteLine(message);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Message written to Console"));
        }

        public static async Task SetupDevConsole()
        {
            writer = new StringWriterExt();
            writer.Flushed += Writer_Flushed;
            timer = new Timer(2000)
            {
                AutoReset = false
            };
            timer.Elapsed += Timer_Elapsed;

            try
            {
                Console.WriteLine("Switching to Custome Console");
                Console.SetOut(writer);
                await Program.DevConsoleRunning();
            }
            finally
            {
                Console.SetOut(defaultConsole);
                Console.WriteLine("Switched to default Console");
            }
        }

        private static void Writer_Flushed(object sender, EventArgs e)
        {
            if (sender is StringWriterExt sw)
            {
                Console.SetOut(defaultConsole);
                Console.WriteLine("Switched to default Console");

                string message = sw.ToString();
                string prefix = $"{DateTime.Now:[HH:mm:ss:fff]}  ";

                if (string.IsNullOrEmpty(message.Trim()))
                {
                    Console.WriteLine($"{prefix}Empty Message");
                }
                else
                {
                    message = prefix + message.Replace("\n", $"\n{prefix}") + "\n";

                    Console.Write(message);
                    AddConsoleMessage(message);
                }

                sw.GetStringBuilder().Clear(); // Clear the buffer after writing to console

                Console.WriteLine("Switching to Custome Console");
                Console.SetOut(writer);
            }
        }

        private static string combinedMessage = "";
        private const int maxMessageLength = 1900;

        private static void AddConsoleMessage(string message)
        {
            lock (lockObject)
            {
                combinedMessage += message;
                timer.Stop();
                timer.Start();
            }
        }

        private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Task.Run(WriteToConsole);
        }

        private static async Task WriteToConsole()
        {
            if (File.Exists($"{Program.ExeFolderPath}/consoleChannels.json"))
            {
                HashSet<DiscordChannel> channels = [];

                string content = File.ReadAllText($"{Program.ExeFolderPath}/consoleChannels.json");
                try
                {
                    ulong[] ids = [];
                    if (!string.IsNullOrEmpty(content))
                    {
                        ids = JsonConvert.DeserializeObject<ulong[]>(content);
                    }
                    foreach (ulong id in ids)
                    {
                        channels.Add(await Bot.Client.GetChannelAsync(id));
                    }
                }
                catch { }
                finally
                {
                    ConsoleChannels = channels;
                }
            }

            if (ConsoleChannels.Count == 0)
            {
                Console.WriteLine("No Console Channels set. Storing Message for later.");
                return;
            }

            string message;

            lock (lockObject)
            {
                message = combinedMessage;
                combinedMessage = "";
            }

            foreach (var channel in ConsoleChannels)
            {
                if (message.Length < maxMessageLength)
                    await channel.SendMessageAsync($"```{message}```");
                else
                {
                    int i = 0;
                    while (i < message.Length)
                    {
                        await channel.SendMessageAsync($"```{message.Substring(i, Math.Min(maxMessageLength, message.Length - i))}```");
                        i += maxMessageLength;

                        //delay 5s
                        await Task.Delay(5000);
                    }
                }
            }
        }

        public static void CommandFailed(DiscordUser user, string commandName, (string parameterName, string value, string limits)[] parameters, string reason, Exception ex = null)
        {
            string message = $"Command {commandName} failed for {user.Username}:{user.Id}\n";

            foreach (var (parameterName, value, limits) in parameters)
            {
                message += $"Parameter {parameterName} Value:  [{value}], AllowedValues/Limits [{limits}]\n";
            }

            message += $"Reason:\n{reason}";

            if (ex != null)
            {
                message += $"\nException:\n{ex.Message}\n{ex.StackTrace}";
            }

            Console.WriteLine(message);
        }

        public static void InteractionFailed(DiscordInteraction interaction, string reason, Exception ex = null)
        {
            string message = $"Interaction in {interaction.Channel.Name} from {interaction.CreationTimestamp} failed for {interaction.User.Username}:{interaction.User.Id}\n";
            message += $"Reason:\n{reason}";

            if (ex != null)
            {
                message += $"\nException:\n{ex.Message}\n{ex.StackTrace}";
            }

            Console.WriteLine(message);
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
