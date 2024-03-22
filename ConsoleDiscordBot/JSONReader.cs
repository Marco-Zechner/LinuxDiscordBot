using Newtonsoft.Json;

namespace DiscordBot
{
    internal class JSONReader
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
        
        public async Task ReadJson()
        {
            using (StreamReader sr = new StreamReader("config.json"))
            {
                string json = await sr.ReadToEndAsync();
                JSONStructure data = JsonConvert.DeserializeObject<JSONStructure>(json);

                Token = data.Token;
                Prefix = data.Prefix;
            }
        }
    }

    internal sealed class JSONStructure
    {
        public string Token { get; set; }
        public string Prefix { get; set; }
    }
}
