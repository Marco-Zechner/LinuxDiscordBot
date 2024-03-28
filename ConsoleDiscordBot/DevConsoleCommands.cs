﻿using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System.Reflection.PortableExecutable;

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
            await ctx.DeferAsync();

            if (consoleChannel == null)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No Channel provided"));

                CommandFailed(ctx.User, "BoxIt", new (string, string, string)[]
                {
                    ("Channel", "null", "channelName"),
                }, "No Channel provided");

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

        public static async Task SetupDevConsole()
        {
            if (File.Exists($"{Program.ExeFolderPath}/consoleChannels.json"))
            {
                HashSet<DiscordChannel> channels = [];

                foreach (ulong id in JsonConvert.DeserializeObject<ulong[]>(File.ReadAllText($"{Program.ExeFolderPath}/consoleChannels.json")))
                {
                    channels.Add(await Bot.Client.GetChannelAsync(id));
                }

                ConsoleChannels = channels;
            }

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

        public static void CommandFailed(DiscordUser user, string commandName, (string parameterName, string value, string limits)[] parameters, string reason, Exception ex = null)
        {
            string message = $"Command {commandName} failed for {user.Username}:{user.Id}\n";

            foreach (var (parameterName, value, limits) in parameters)
            {
                message += $"Parameter {parameterName} failed. [{value}] : AllowedValues/Limits [{limits}]\n";
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
