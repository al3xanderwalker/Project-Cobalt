using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Project_Cobalt.Utils
{
    public static class ConfigHelper
    {
        public static void EnsureConfig(string path, JObject defaultConfig)
        {
            if (File.Exists(path)) return;
            
            using (var file = File.CreateText(path))
            using (var writer = new JsonTextWriter(file))
            {
                defaultConfig.WriteTo(writer);
            }
        }

        public static T ReadConfig<T>(string path)
        {
            using (var file = File.OpenText(path))
            using (var reader = new JsonTextReader(file))
            {
                return JsonConvert.DeserializeObject<T>(JToken.ReadFrom(reader).ToString());
            }
        }
    }
}