namespace Project_Cobalt.Models
{
    public class ItemSlot
    {
        public byte XPos;
        public bool Clear;
        
        public ItemSlot(byte xPos, bool clear)
        {
            XPos = xPos;
            Clear = clear;
        }
    }
}