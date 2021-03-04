using System.Collections.Generic;
using Project_Cobalt.Models;
using SDG.Unturned;
using UnityEngine;

namespace rust_unturned_module.API.Junkpiles
{
    public class Junkpile
    {
        public List<Lootable> Lootables = new List<Lootable>();
        public Transform JunkTransform;

        public Junkpile(Vector3 position)
        {
            position.y = LevelGround.getHeight(position) + 0.2f;

            var loot1 = Random.Range(0f, 1f) > 0.75f ? LootType.Crate0 : LootType.None;
            if (loot1 != 0) Lootables.Add(SpawnLootable(loot1, position, new Vector3(-2.6f, 0.85f, 0.4f)));
            var loot2 = IntToLootType(Random.Range(1, 4));
            if (loot2 != 0) Lootables.Add(SpawnLootable(loot2, position, new Vector3(-1f, 0f, 2f)));
            var loot3 = IntToLootType(Random.Range(1, 4));
            if (loot3 != 0) Lootables.Add(SpawnLootable(loot3, position, new Vector3(-0.1f, 0f, 1.3f)));
            var loot4 = IntToLootType(Random.Range(1, 4));
            if (loot4 != 0) Lootables.Add(SpawnLootable(loot4, position, new Vector3(-1.2f, 0f, -0.5f)));
            position.y += 0.85f;
            JunkTransform = DropBarricade(position, 46437);
        }


        private static LootType IntToLootType(int num)
        {
            switch (num)
            {
                case 1:
                    return LootType.Barrel0;
                case 2:
                    return LootType.Barrel1;
                case 3:
                    return LootType.Barrel2;
                case 4:
                    return LootType.Crate0;
                default:
                    return LootType.None;
            }
        }

        public void Despawn()
        {
            if (!BarricadeManager.tryGetInfo(JunkTransform, out var x, out var y, out var plant,
                out var index,
                out var region)) return;
            BarricadeManager.destroyBarricade(region, x, y, plant, index);
        }


        private static Transform DropBarricade(Vector3 position, ushort id) => BarricadeManager.dropBarricade(
            new Barricade(id), null,
            position, 0f, 0f, 0f, 0UL, 0UL);

        private static Lootable SpawnLootable(LootType lootType, Vector3 position, Vector3 displacement)
        {
            position += displacement;
            return new Lootable(lootType, DropBarricade(position, (ushort) lootType));
        }
    }
}