using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System.Reflection;

namespace ConsoleDiscordBot
{
    public class Updater : ApplicationCommandModule
    {
        // major.minor.hotfix
        // major - a feature completed
        // minor - new features added
        // hotfix - bug fixes, small changes
        public const string Version = "1.2.0";
        class UpdateBotInfo
        {
            public ulong ChannelID { get; set; }
            public string CurrentVersion { get; set; }
        }

        [SlashCommand("BeamMeUpScotty", "Tries to update the Bot")]
        public static async Task UpdateBot(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            UpdateBotInfo info = new()
            {
                ChannelID = ctx.Channel.Id,
                CurrentVersion = Version
            };

            string exeFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            File.WriteAllText($"{exeFolderPath}/updateBotInfo.json", JsonConvert.SerializeObject(info));

            string infoBox = CodeBoxDrawer.DrawBoxWithHeader($"{info.CurrentVersion}", $"Bot will now restart and attempt to update.\nThis will take around 1 minute.");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Description = $"```{infoBox}```"
                })
                );
            
            Environment.Exit(1);
        }

        [SlashCommand("VersionInfo", "Prints the current Version of the Bot.")]
        public static async Task VersionInfo(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Bot currently on Version {Version}"));
        }

        public static async Task AfterStartUp()
        {
            string exeFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (File.Exists($"{exeFolderPath}/updateBotInfo.json"))
            {
                UpdateBotInfo info = JsonConvert.DeserializeObject<UpdateBotInfo>(File.ReadAllText($"{exeFolderPath}/updateBotInfo.json"));

                DiscordChannel channel = await Bot.Client.GetChannelAsync(info.ChannelID);
                await channel.SendMessageAsync($"Bot has been updated from {info.CurrentVersion} to Version {Version}");

                File.Delete($"{exeFolderPath}/updateBotInfo.json");
            }
        }
    }
}
