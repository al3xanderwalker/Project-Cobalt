using HarmonyLib;
using Project_Cobalt.Models;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCBarricadeManager : MonoBehaviour, IObjectComponent
    {
        private void Awake()
        {
            PCLogManager.LogLoaded("Stacking Manager Loaded");
            BarricadeManager.onSalvageBarricadeRequested += OnSalvageBarricadeRequested;
        }

        private static void OnSalvageBarricadeRequested(CSteamID steamID, byte x, byte y, ushort plant, ushort index,
            ref bool allow)
        {
            allow = false;
            var steamPlayer = PlayerTool.getSteamPlayer(steamID);

            if (!BarricadeManager.tryGetRegion(x, y, plant, out var barricadeRegion)) return;
            var itemBarricadeAsset =
                (ItemBarricadeAsset) Assets.find(EAssetType.ITEM, barricadeRegion.barricades[index].barricade.id);
            if (itemBarricadeAsset == null) return;
            if (itemBarricadeAsset.isUnpickupable) return;
            PCSplitterManager.AddItemStacked(steamPlayer, barricadeRegion.barricades[index].barricade.id, 1);
            BarricadeManager.destroyBarricade(barricadeRegion, x, y, plant, index);
        }
    }

    [HarmonyPatch(typeof(PlayerEquipment), "use")]
    internal class UsePatch
    {
        public static void Prefix(PlayerEquipment __instance)
        {
            var player = __instance.channel.owner.player;
            if (!__instance.isSelected) return;

            var itemID = __instance.itemID;
            var index = player.inventory.getIndex(__instance.equippedPage, __instance.equipped_x,
                __instance.equipped_y);
            var item = player.inventory.getItem(__instance.equippedPage, index);
            var equippedPage = __instance.equippedPage;
            var equippedX = __instance.equipped_x;
            var equippedY = __instance.equipped_y;
            var rot = item.rot;
            if (item.item.amount <= 1) player.inventory.removeItem(__instance.equippedPage, index);
            else player.inventory.sendUpdateAmount(equippedPage, equippedX, equippedY, (byte) (item.item.amount - 1));
            __instance.dequip();
            var inventorySearch = player.inventory.has(itemID);
            if (inventorySearch == null) return;
            __instance.tryEquip(equippedPage, equippedX, equippedY);
        }
    }
}