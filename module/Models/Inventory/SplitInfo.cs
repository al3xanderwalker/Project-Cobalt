using System.Collections.Generic;
using SDG.Unturned;

namespace Project_Cobalt.Models
{
    public class SplitInfo
    {
        public Item Item;
        public EStage Stage;
        public List<ItemSlot> Slots;
        
        public SplitInfo(Item item, EStage stage)
        {
            Item = item;
            Stage = stage;
            Slots = new List<ItemSlot>();
        }

        public SplitInfo(Item item, EStage stage, List<ItemSlot> slots)
        {
            Item = item;
            Stage = stage;
            Slots = slots;
        }
    }
}