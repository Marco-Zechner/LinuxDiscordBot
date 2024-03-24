using DSharpPlus.Interactivity;
using DSharpPlus;
using DiscordBot;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using System.Diagnostics.Tracing;

namespace ConsoleDiscordBot
{
    public class Bot
    {
        public static DiscordClient Client { get; private set; }

        public static async Task Setup()
        {
            if (Client?.IsConnected == true)
            {
                return;
            }
            await JSONReader.ReadJson();

            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = JSONReader.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            Client.SessionCreated += Client_SessionCreated;
            
            SlashCommandsExtension? slashCommandsConfig = Client.UseSlashCommands();
            DiscordBotEventsSetup.BeforeConnect(slashCommandsConfig);

            await Client.ConnectAsync();

            await Updater.AfterStartUp();

            await Task.Delay(-1);
        }

        private static Task Client_SessionCreated(DiscordClient sender, SessionReadyEventArgs args)
        {
            return Task.CompletedTask;
        }
    }
}
