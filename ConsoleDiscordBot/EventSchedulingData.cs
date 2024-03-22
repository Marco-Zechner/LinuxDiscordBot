using DSharpPlus;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace ConsoleDiscordBot
{
    public class EventSchedulingData(string topic, string topicURL, DateTime suggestedDateTime, int maxTimeOffset, DiscordRole role, DiscordAttachment background)
    {
        public string Topic { get; } = topic;
        public string TopicURL { get; } = topicURL;
        public DateTime SuggestedDateTime { get; } = suggestedDateTime;
        public int MaxTimeOffset { get; } = maxTimeOffset;
        public DiscordRole Role { get; } = role;
        [JsonIgnore]
        public string? BackgroundUrl { get; set; } = background?.Url;

        public readonly HashSet<EventUser> eventUsers = [];

        public async Task<DiscordWebhookBuilder> CreateMessage(DiscordInteraction interaction)
        {
            string dataString = DataHandler<EventSchedulingData>.AddDataAndGetString(interaction, this);

            Console.WriteLine("Created Data");

            //create a temp txt file
            await File.WriteAllTextAsync("temp.txt", dataString);

            FileStream fileStream = new("temp.txt", FileMode.Open);

            Console.WriteLine("Wrote Data to a File");

            var message = new DiscordWebhookBuilder()
                .AddEmbed(CreateEmbed())
                .AddComponents(CreateButtons())
                .AddComponents(CreateDropdown())
                .AddFile("SPOILER_EventSchedulingData.fuckyou", fileStream, AddFileOptions.CloseStream)
                ;

            return message;
        }

        public async Task UpdateInteraction(DiscordInteraction interaction)
        {
            string dataString = DataHandler<EventSchedulingData>.AddDataAndGetString(interaction, this);

            //create a temp txt file
            await File.WriteAllTextAsync("temp2.txt", dataString);

            FileStream fileStream = new("temp2.txt", FileMode.Open);

            await interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                .AddEmbed(CreateEmbed())
                .AddComponents(CreateButtons())
                .AddComponents(CreateDropdown())
                .AddFile("SPOILER_EventSchedulingData.fuckyou", fileStream, AddFileOptions.CloseStream)
            );

            fileStream.Close();
        }

        private DiscordEmbedBuilder CreateEmbed()
        {

            var embed = new DiscordEmbedBuilder
            {
                Title = $"{SuggestedDateTime:dd.MM.yyyy}",
                Description = $"# {Topic}",
                Color = DiscordColor.Orange,
            };

            if (string.IsNullOrEmpty(BackgroundUrl) == false)
            {
                embed.ImageUrl = BackgroundUrl;
            }

            foreach (var fieldData in CreateFields())
            {
                embed.AddField(fieldData.Key, fieldData.Value, true);
            }
            return embed;
        }

        private Dictionary<string, string> CreateFields()
        {
            Dictionary<string, string> fieldsToAdd = [];

            foreach (var eventUser in eventUsers)
            {

                string fieldNameToAddTo = "Marco messed Up";
                string fieldValueToAdd = $"{eventUser.UserMention}\n";

                switch (eventUser.State)
                {
                    case EventUserState.Registered:
                        fieldNameToAddTo = eventUser.SelectedTime.ToString("HH:mm");
                        //if (eventUser.SelectedTime.Date != SuggestedDateTime.Date)
                            fieldNameToAddTo += eventUser.SelectedTime.ToString(" dd.MM");
                        break;
                    case EventUserState.Expected:
                        fieldNameToAddTo = "Expected";
                        break;
                    case EventUserState.Declined:
                        fieldNameToAddTo = "Declined";
                        break;
                    case EventUserState.Undecided:
                        fieldNameToAddTo = "Undecided";
                        break;
                }


                if (fieldsToAdd.TryGetValue(fieldNameToAddTo, out var value))
                {
                    fieldsToAdd[fieldNameToAddTo] = value + fieldValueToAdd;
                    continue;
                }

                fieldsToAdd.Add(fieldNameToAddTo, fieldValueToAdd);
            }

            return fieldsToAdd;
        }

        private DiscordComponent[] CreateButtons()
        {
            var minus30Min = new DiscordButtonComponent(ButtonStyle.Secondary, "eventScheduling_minus30", "<═ 30");
            var acceptButton = new DiscordButtonComponent(ButtonStyle.Success, "eventScheduling_accept", SuggestedDateTime.ToString("HH:mm"));
            var plus30Min = new DiscordButtonComponent(ButtonStyle.Secondary, "eventScheduling_plus30", "30 ═>");
            var declineButton = new DiscordButtonComponent(ButtonStyle.Danger, "eventScheduling_decline", "Decline");
            var maybeButton = new DiscordButtonComponent(ButtonStyle.Primary, "eventScheduling_undecided", "Undecided");

            return [minus30Min, acceptButton, plus30Min, maybeButton, declineButton];
        }

        private DiscordSelectComponent CreateDropdown()
        {
            int offsetHalfHoursToDisplay = MaxTimeOffset * 2;
            DateTimeOffset startTime = SuggestedDateTime.AddMinutes(-offsetHalfHoursToDisplay * 30);

            var optionsDropdown = new List<DiscordSelectComponentOption>();

            for (int i = 0; i < offsetHalfHoursToDisplay * 2 + 1; i++)
            {
                string option = startTime.ToString("HH:mm");

                if (startTime.Date != SuggestedDateTime.Date)
                    option += " " + startTime.ToString("dd.MM");

                if (startTime >= DateTimeOffset.Now)
                    optionsDropdown.Add(new DiscordSelectComponentOption(option, option));

                startTime = startTime.AddMinutes(30);
            }

            var dropdown = new DiscordSelectComponent("eventScheduling_time", "Select an option", optionsDropdown, false, 0, 1);

            return dropdown;
        }

        public void RegisterUser(DiscordMember user, DateTime selectedTime)
        {
            EventUser eventUser = eventUsers.FirstOrDefault(eu => eu.UserMention.Equals(user.Mention), null) ?? new EventUser(user.Mention, SuggestedDateTime);

            eventUser.State = EventUserState.Registered;
            eventUser.SelectedTime = selectedTime;

            eventUsers.Add(eventUser);
        }

        public void DelayUser(DiscordMember user, int minutes)
        {
            EventUser eventUser = eventUsers.FirstOrDefault(eu => eu.UserMention.Equals(user.Mention), null) ?? new EventUser(user.Mention, SuggestedDateTime);

            eventUser.State = EventUserState.Registered;
            eventUser.SelectedTime = eventUser.SelectedTime.AddMinutes(minutes);

            eventUsers.Add(eventUser);
        }

        public void UnregisterUser(DiscordMember user)
        {
            EventUser eventUser = eventUsers.FirstOrDefault(eu => eu.UserMention.Equals(user.Mention), null) ?? new EventUser(user.Mention, SuggestedDateTime);

            eventUser.State = EventUserState.Declined;

            eventUsers.Add(eventUser);
        }

        public void UndecidedUser(DiscordMember user)
        {
            EventUser eventUser = eventUsers.FirstOrDefault(eu => eu.UserMention.Equals(user.Mention), null) ?? new EventUser(user.Mention, SuggestedDateTime);

            eventUser.State = EventUserState.Undecided;

            eventUsers.Add(eventUser);
        }

        public void ExpectUser(DiscordMember user)
        {
            EventUser eventUser = eventUsers.FirstOrDefault(eu => eu.UserMention.Equals(user.Mention), null) ?? new EventUser(user.Mention, SuggestedDateTime);

            eventUser.State = EventUserState.Expected;

            eventUsers.Add(eventUser);
        }
    }
}
