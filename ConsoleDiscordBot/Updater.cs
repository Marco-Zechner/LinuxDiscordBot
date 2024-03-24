using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;
using System.Reflection;

namespace ConsoleDiscordBot
{
    public class Updater : ApplicationCommandModule
    {
        private static readonly UpdateBotInfo CurrentInfo = new()
        {
            ChannelID = 0,
            VersionMajor = 1,
            VersionMinor = 2,
            VersionHotfix = 4
        };

        class UpdateBotInfo
        {
            public ulong ChannelID { get; set; }
            public int VersionMajor { get; set; }
            public int VersionMinor { get; set; }
            public int VersionHotfix { get; set; }

            public string Version => $"{VersionMajor}.{VersionMinor}.{VersionHotfix}";
            
            public UpdateBotInfo() { }
            
            public UpdateBotInfo(string CurrentVersion)
            {
                string[] version = CurrentVersion.Split('.');
                VersionMajor = int.Parse(version[0]);
                VersionMinor = int.Parse(version[1]);
                VersionHotfix = int.Parse(version[2]);
            }

            public override bool Equals(object? obj)
            {
                if (obj is UpdateBotInfo info)
                {
                    return info.VersionMajor == VersionMajor && info.VersionMinor == VersionMinor && info.VersionHotfix == VersionHotfix;
                }
                return false;
            }

            public override int GetHashCode()
            {
                return VersionMajor.GetHashCode() + VersionMinor.GetHashCode() + VersionHotfix.GetHashCode();
            }

            public override string ToString()
            {
                return $"{VersionMajor}.{VersionMinor}.{VersionHotfix}";
            }

            public static bool operator ==(UpdateBotInfo left, UpdateBotInfo right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(UpdateBotInfo left, UpdateBotInfo right)
            {
                return !left.Equals(right);
            }

            public static bool operator >(UpdateBotInfo left, UpdateBotInfo right)
            {
                if (left.VersionMajor > right.VersionMajor)
                {
                    return true;
                }
                else if (left.VersionMajor == right.VersionMajor)
                {
                    if (left.VersionMinor > right.VersionMinor)
                    {
                        return true;
                    }
                    else if (left.VersionMinor == right.VersionMinor)
                    {
                        if (left.VersionHotfix > right.VersionHotfix)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }

            public static bool operator <(UpdateBotInfo left, UpdateBotInfo right)
            {
                if (left.VersionMajor < right.VersionMajor)
                {
                    return true;
                }
                else if (left.VersionMajor == right.VersionMajor)
                {
                    if (left.VersionMinor < right.VersionMinor)
                    {
                        return true;
                    }
                    else if (left.VersionMinor == right.VersionMinor)
                    {
                        if (left.VersionHotfix < right.VersionHotfix)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        [SlashCommand("BeamMeUpScotty", "Tries to update the Bot")]
        public static async Task UpdateBot(InteractionContext ctx)
        {
            await ctx.DeferAsync();

            CurrentInfo.ChannelID = ctx.Channel.Id;

            string exeFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            File.WriteAllText($"{exeFolderPath}/updateBotInfo.json", JsonConvert.SerializeObject(CurrentInfo));

            string infoBox = CodeBoxDrawer.DrawBoxWithHeader($"{CurrentInfo}", $"Bot will now restart and attempt to update.\nThis will take around 1 minute.");

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

            string infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Bot", $"Current Version: {CurrentInfo}");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Description = $"```{infoBox}```"
                })
                );
        }

        public static async Task AfterStartUp()
        {
            string exeFolderPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            if (File.Exists($"{exeFolderPath}/updateBotInfo.json"))
            {
                UpdateBotInfo info = JsonConvert.DeserializeObject<UpdateBotInfo>(File.ReadAllText($"{exeFolderPath}/updateBotInfo.json"));

                DiscordChannel channel = await Bot.Client.GetChannelAsync(info.ChannelID);

                string infoBox = "";

                if (CurrentInfo < info)
                {
                    infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Result", $"Bot downgraded from {info} to {CurrentInfo}");
                }
                else if (CurrentInfo > info)
                {
                    infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Result", $"Bot upgraded from {info} to {CurrentInfo}");
                }
                else
                {
                    infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Result", $"No other Version found.\nStayed on {CurrentInfo}");
                }

                await channel.SendMessageAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Description = $"```{infoBox}```"
                    })
                    );


                File.Delete($"{exeFolderPath}/updateBotInfo.json");
            }
        }
    }
}
