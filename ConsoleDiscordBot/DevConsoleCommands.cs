using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class DevConsoleCommands : ApplicationCommandModule
    {
        static readonly TextWriter defaultConsole = Console.Out;
        static StringWriterExt writer;
        public static List<ulong> ActiveDevIDs { get; set; } = [];


        [SlashCommand("DevConsole", "Send the Console output to the User")]
        public static async Task DevConsole(InteractionContext ctx,
            [Option("active", "If the bot should send the stuff to you")] bool active 
            )
        {
            await ctx.DeferAsync();

            if (active)
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                    .WithContent("I will send you the Console output")
                    .AsEphemeral(true)
                    );
                ActiveDevIDs.Add(ctx.User.Id);
            }
            else
            {
                await ctx.CreateResponseAsync(DSharpPlus.InteractionResponseType.ChannelMessageWithSource, new DSharpPlus.Entities.DiscordInteractionResponseBuilder()
                    .WithContent("I will stop sending you the Console output")
                    .AsEphemeral(true)
                    );
                ActiveDevIDs.Remove(ctx.User.Id);
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
            //pm users in ActiveDevIDs
            //foreach (var id in ActiveDevIDs)
            //{
            //    var user = await Bot.Client.GetUserAsync(id);

            //    DiscordMember member = (DiscordMember)user;
            //    var channel = await member.CreateDmChannelAsync();

            //    await channel.SendMessageAsync(message);
            //}
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
