namespace Project_Cobalt.Models
{
    public class BuildCost
    {
        public ushort Id;
        public string Name;
        public int Cost;

        public BuildCost(ushort id, string name, int cost)
        {
            Id = id;
            Name = name;
            Cost = cost;
        }
    }
}