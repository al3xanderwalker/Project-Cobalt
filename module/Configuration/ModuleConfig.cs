using System.IO;
using System.Reflection;
using Project_Cobalt.Models;
using Project_Cobalt.Utils;

namespace Project_Cobalt.API
{
    public static class ModuleConfig
    {
        public static void LoadConfigs()
        {
            var split = Path.DirectorySeparatorChar;
            var main = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var configDir = $"{main}{split}Config{split}";
            
            ConfigHelper.EnsureConfig($"{configDir}MainConfig.json", MainConfig.DefaultConfig);
            ConfigHelper.EnsureConfig($"{configDir}Build_Cost.json", BuildCostConfig.DefaultConfig);

            Main.Config = ConfigHelper.ReadConfig<MainConfig>($"{configDir}MainConfig.json");
            Main.BuildCostConfig = ConfigHelper.ReadConfig<BuildCostConfig>($"{configDir}Build_Cost.json");
        }
    }
}