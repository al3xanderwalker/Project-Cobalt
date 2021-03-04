using UnityEngine;

namespace Project_Cobalt.Models
{
    public class Lootable
    {
        public LootType LootType;
        public Transform Transform;

        public Lootable(LootType lootType, Transform transform)
        {
            LootType = lootType;
            Transform = transform;
        }
    }
}