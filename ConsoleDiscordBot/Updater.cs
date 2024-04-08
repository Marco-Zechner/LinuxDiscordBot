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
            VersionMinor = 3,
            VersionHotfix = 14,
            Changes = @"
- added switch from embed to message if codeblock is to wide (for updater)
"
        };

        class UpdateBotInfo
        {
            public ulong ChannelID { get; set; }
            public int VersionMajor { get; set; }
            public int VersionMinor { get; set; }
            public int VersionHotfix { get; set; }
            public int SleepTime { get; set; }
            public string Changes { get; set; }

            public string Version => $"{ VersionMajor}.{VersionMinor}.{VersionHotfix}";
            
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
        public static async Task UpdateBot(InteractionContext ctx,
            [Option("Minutes", "How long the Bot should sleep.")] double minutes = 0)
        {
            await ctx.DeferAsync();

            CurrentInfo.ChannelID = ctx.Channel.Id;
            CurrentInfo.SleepTime = (int)minutes;

            File.WriteAllText($"{Program.ExeFolderPath}/updateBotInfo.json", JsonConvert.SerializeObject(CurrentInfo));

            string infoText = $"Bot will now restart and attempt to update.\nThis will take around 30 seconds.";

            if (minutes > 0)
            {
                infoText += $"\nBot will sleep for {minutes} minutes after the update.";
            }

            string infoBox = CodeBoxDrawer.DrawBoxWithHeader($"{CurrentInfo}", infoText);

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
            Console.WriteLine("Printed VersionInfo");

            string infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Bot", $"Current Version: {CurrentInfo}");

            await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                {
                    Description = $"```{infoBox}```"
                })
                );
        }

        public static async Task CheckForSleeping()
        {
            if (File.Exists($"{Program.ExeFolderPath}/updateBotInfo.json"))
            {
                UpdateBotInfo info = JsonConvert.DeserializeObject<UpdateBotInfo>(File.ReadAllText($"{Program.ExeFolderPath}/updateBotInfo.json"));

                if (info.SleepTime <= 0)
                {
                    return;
                }

                Console.WriteLine($"Bot is sleeping for {info.SleepTime} minutes.");

                await Task.Delay((int)(info.SleepTime * 60000));
            }
        }

        public static async Task AfterConnect()
        {
            if (File.Exists($"{Program.ExeFolderPath}/updateBotInfo.json"))
            {
                string content = File.ReadAllText($"{Program.ExeFolderPath}/updateBotInfo.json");
                UpdateBotInfo info = JsonConvert.DeserializeObject<UpdateBotInfo>(content);

                DiscordChannel channel = await Bot.Client.GetChannelAsync(info.ChannelID);

                string infoBox;
                if (CurrentInfo < info)
                {
                    infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Result", $"Bot rolled back from {info} to {CurrentInfo}");
                }
                else if (CurrentInfo > info)
                {
                    infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Result", $"Bot updated from {info} to {CurrentInfo}");
                }
                else
                {
                    infoBox = CodeBoxDrawer.DrawBoxWithHeader($"Result", $"No other Version found.\nStayed on {CurrentInfo}");
                }

                await File.WriteAllTextAsync("temp.txt", CurrentInfo.Changes);

                FileStream fileStream = new("temp.txt", FileMode.Open);

                if (infoBox.Split('\n')[0].Length > 60)
                {
                    await channel.SendMessageAsync(new DiscordMessageBuilder()
                    .WithContent($"```{infoBox}```")
                    .AddFile("changes.txt", fileStream, AddFileOptions.CloseStream)
                    );
                }
                else
                {
                    await channel.SendMessageAsync(new DiscordMessageBuilder()
                    .AddEmbed(new DiscordEmbedBuilder()
                    {
                        Description = $"```{infoBox}```"
                    })
                    .AddFile("changes.txt", fileStream, AddFileOptions.CloseStream)
                    );
                }




                File.Delete($"{Program.ExeFolderPath}/updateBotInfo.json");
            }

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
                    DevConsoleCommands.ConsoleChannels = channels;
                }
            }
        }
    }
}
