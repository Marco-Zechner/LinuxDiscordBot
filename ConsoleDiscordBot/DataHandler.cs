using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace ConsoleDiscordBot
{
    public static class DataHandler<T> where T : class
    {
        private static string ConvertDataToString(T data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
        private static T ConvertStringToData(string data)
        {
            return JsonConvert.DeserializeObject<T>(data);
        }

        private static readonly Dictionary<ulong, T> store = [];

        public static void Add(ulong id, T data)
        {
            store[id] = data;
        }

        public static T? Get(ulong id)
        {
            store.TryGetValue(id, out T? data);
            return data;
        }

        // Adds or updates a data entry based on the interaction ID
        public static string AddDataAndGetString(DiscordInteraction interaction, T data)
        {
            Add(interaction.Id, data);
            return ConvertDataToString(data);
        }

        // Retrieves data based on ComponentInteractionCreateEventArgs, including logic for missing data
        public static T? GetData(DiscordInteraction interaction, string dataString)
        {
            T? data = Get(interaction.Id);
            if (data == null)
            {
                if (string.IsNullOrEmpty(dataString)) return null;

                data = ConvertStringToData(dataString);
                if (data != null)
                {
                    Add(interaction.Id, data);
                }
            }
            return data;
        }
    }
}
