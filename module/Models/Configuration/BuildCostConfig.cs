using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Project_Cobalt.Models
{
    public class BuildCostConfig
    {
        public List<BuildCost> Wood { get; set; }
        
        public static JObject DefaultConfig = new JObject()
        {
            {"Wood", JToken.FromObject(new List<BuildCost>()
            {
                new BuildCost(23000,"example",100)
            })}
        };
    }
}