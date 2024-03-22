using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace ConsoleDiscordBot
{
    public class EventScheduling : ApplicationCommandModule
    {

        [SlashCommand("ScheduleEvent", "Create a Poll where users can select there pefered time")]
        public static async Task ScheduleEvent(InteractionContext ctx,
            [Option("topic", "Topic of the Vot")] string topic,
            [Option("suggestedTime", "The suggestedTime for the topic")] string suggestedTime,
            [Option("date", "The date of the topic")] string date = "",
            [Option("role", "The role who's users are expected to vote")] DiscordRole role = null,
            [Option("background", "Large Image Below")] DiscordAttachment background = null,
            [Option("topicURL", "A internet link to the topic")] string topicURL = null,
            [Option("maxTimeShiftHours", "How many hours earlier or later is possible to select")] long maxTimeShiftHours = 4
            )
        {
            await ctx.DeferAsync();

            Console.WriteLine("Creating new EventScheduling");

            if (string.IsNullOrEmpty(date))
            {
                date = DateTime.Now.ToString("dd.MM.yyyy");
            }
            
            if (!DateTime.TryParse($"{date} {suggestedTime}", out DateTime suggestedDateTime))
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Invalid Time \"{suggestedTime}\" or Date \"{date}\". (date is optional)")
                );
                return;
            }

            if (maxTimeShiftHours < 2) maxTimeShiftHours = 2;

            var validImages = await EmbedImageValidation.Validate(ctx, background);
            if (validImages == false) return;

            Console.WriteLine("Attributes are valid");

            DiscordMember? creator = ctx.User as DiscordMember;

            var poll = new EventSchedulingData(topic, topicURL, suggestedDateTime, (int)maxTimeShiftHours, role, background);

            if (creator?.Roles.Contains(role) == true)
            {
                poll.RegisterUser(creator, suggestedDateTime);
            }
            var members = ctx.Guild.GetAllMembersAsync().ToBlockingEnumerable();

            foreach (var user in members.Where(m => m.Roles.Contains(role)))
            {
                poll.ExpectUser(user);
            }

            Console.WriteLine("Set up Role and Creator");

            var message = await poll.CreateMessage(ctx.Interaction);

            Console.WriteLine("Created Message");

            await ctx.EditResponseAsync(message);

            Console.WriteLine("Updated Placeholder");
        }

        public static async Task HandleInteraction(DiscordMessage message, DiscordInteraction interaction, string interactionID)
        {
            string dataString = "";

            foreach (var attachment in message.Attachments)
            {
                if (attachment.FileName == "SPOILER_EventSchedulingData.fuckyou")
                {
                    var fileURL = attachment.Url;
                    var fileContent = await new HttpClient().GetStringAsync(fileURL);
                    dataString = fileContent;
                    break;
                }
            }

            EventSchedulingData? poll = DataHandler<EventSchedulingData>.GetData(interaction, dataString);

            if (poll == null)
            {
                await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("The Poll has expired or is not valid anymore")
                .AsEphemeral(true)
                );
                return;
            }

            if (interaction.User is not DiscordMember user)
            {
                await interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("You are an invalid User. Please contact the Bot owner")
                .AsEphemeral(true)
                );
                return;
            }

            if (message.Embeds[0].Image?.Url != null)
            {
                poll.BackgroundUrl = message.Embeds[0].Image.Url.ToString();
            }
            
            switch (interactionID)
            {
                case "eventScheduling_minus30":
                    poll.DelayUser(user, -30);
                    break;
                case "eventScheduling_accept":
                    poll.RegisterUser(user, poll.SuggestedDateTime);
                    break;
                case "eventScheduling_plus30":
                    poll.DelayUser(user, 30);
                    break;
                case "eventScheduling_decline":
                    poll.UnregisterUser(user);
                    break;
                case "eventScheduling_undecided":
                    poll.UndecidedUser(user);
                    break;
                case "eventScheduling_time":
                    if (interaction.Data.Values.Length != 1)
                        break;
                    poll.RegisterUser(user, DateTime.Parse(interaction.Data.Values[0]));
                    break;
            }
            
            await poll.UpdateInteraction(interaction);
        }
    }
}
