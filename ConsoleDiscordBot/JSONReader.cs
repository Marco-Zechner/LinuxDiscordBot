using Newtonsoft.Json;

namespace DiscordBot
{
    internal class JSONReader
    {
        public static string Token { get; set; }
        public static string Prefix { get; set; }


        public static async Task ReadJson()
        {
            using StreamReader sr = new("config.json");

            string json = await sr.ReadToEndAsync();
            JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

            Token = data.Token;
            Prefix = data.Prefix;
        }
    }

    internal sealed class JSONStructure
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
    }
}
