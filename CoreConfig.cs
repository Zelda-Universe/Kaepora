using Newtonsoft.Json;
using System.IO;

namespace Kaepora
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CoreConfig
    {
        [JsonProperty("token")]
        public string Token { get; set; } = "YOUR TOKEN HERE";

        public static CoreConfig ReadConfig()
        {
            if (!File.Exists("config.json"))
            {
                File.WriteAllText("config.json", JsonConvert.SerializeObject(new CoreConfig(), Formatting.Indented));
                throw new FileNotFoundException("Config File Not Found - Generating a template at 'config.json'");
            }

            var jsonString = File.ReadAllText("config.json");
            
            return JsonConvert.DeserializeObject<CoreConfig>(jsonString);
        }
    }
}