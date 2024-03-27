using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Newtonsoft.Json;

namespace ConsoleDiscordBot
{
    public class Updater : ApplicationCommandModule
    {
        private static readonly UpdateBotInfo CurrentInfo = new()
        {
            ChannelID = 0,
            VersionMajor = 1,
            VersionMinor = 2,
            VersionHotfix = 6
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

            File.WriteAllText($"{Program.ExeFolderPath}/updateBotInfo.json", JsonConvert.SerializeObject(CurrentInfo));

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

        [SlashCommand("Sleep", "Let's the Bot sleep for x minutes. During this time you can use the local Version of the Bot.")]
        public static async Task Sleep(InteractionContext ctx, [Option("Minutes", "How long the Bot should sleep.")] int minutes)
        {
            await ctx.DeferAsync();

            File.WriteAllText($"{Program.ExeFolderPath}/sleeper.txt", JsonConvert.SerializeObject(minutes));

            string infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Sleep", $"Bot will now sleep for {minutes} minutes.");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Description = $"```{infoBox}```"
                })
                );

            Environment.Exit(1);
        }

        public static async Task CheckForSleeping()
        {
            if (File.Exists($"{Program.ExeFolderPath}/sleeper.txt"))
            {
                int minutes = JsonConvert.DeserializeObject<int>(File.ReadAllText($"{Program.ExeFolderPath}/sleeper.txt"));

                Console.WriteLine($"Bot is sleeping for {minutes} minutes.");

                await Task.Delay(minutes * 60000);

                File.Delete($"{Program.ExeFolderPath}/sleeper.txt");
            }
        }

        public static async Task AfterConnect()
        {
            if (File.Exists($"{Program.ExeFolderPath}/updateBotInfo.json"))
            {
                UpdateBotInfo info = JsonConvert.DeserializeObject<UpdateBotInfo>(File.ReadAllText($"{Program.ExeFolderPath}/updateBotInfo.json"));

                DiscordChannel channel = await Bot.Client.GetChannelAsync(info.ChannelID);

                string infoBox;
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

                File.Delete($"{Program.ExeFolderPath}/updateBotInfo.json");
            }
        }
    }
}
