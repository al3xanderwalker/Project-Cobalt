using Newtonsoft.Json.Linq;

namespace Project_Cobalt.Models
{
    public class MainConfig
    {
        public string Example { get; set; }
        
        public static JObject DefaultConfig = new JObject()
        {
            {"Example", "ChangeMe"}
        };
    }
}