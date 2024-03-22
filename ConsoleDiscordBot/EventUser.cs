namespace ConsoleDiscordBot
{
    public class EventUser(string userMention, DateTime defaultTime)
    {
        public string UserMention { get; } = userMention;
        public DateTime SelectedTime { get; set; } = defaultTime;
        public EventUserState State { get; set; }

        public override int GetHashCode()
        {
            return UserMention.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is EventUser user && user.UserMention.Equals(UserMention);
        }
    }

    public enum EventUserState
    {
        Expected,
        Registered,
        Declined,
        Undecided
    }

}
