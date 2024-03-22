using DSharpPlus.EventArgs;
using DSharpPlus;

namespace ConsoleDiscordBot
{
    public class InteractionsHandler
    {
        public static async Task Client_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs args)
        {
            string interactionID = args.Interaction.Data.CustomId;
            string IDPrefix = interactionID.Split('_')[0];

            switch (IDPrefix)
            {
                case "eventScheduling":
                    await EventScheduling.HandleInteraction(args.Message, args.Interaction, interactionID);
                    break;
                default:
                    break;
            }
        }
    }
}
