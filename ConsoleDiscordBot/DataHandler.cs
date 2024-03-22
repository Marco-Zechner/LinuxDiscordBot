using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace ConsoleDiscordBot
{
    public static class DataHandler<T> where T : class
    {
        private const string prefix = "";
        private const string suffix = "";
        private static string ConvertDataToString(T data)
        {
            //convert to json
            string json = JsonConvert.SerializeObject(data, Formatting.Indented);

            ////convert to base64 string
            //byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);

            //string base64 = Convert.ToBase64String(bytes);


            return $"{prefix}{json}{suffix}";
        }
        private static T ConvertStringToData(string data)
        {
            string base64 = data[prefix.Length..][..^suffix.Length];
            //byte[] bytes = Convert.FromBase64String(base64);

            //string json = System.Text.Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject<T>(base64);
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
