using System;
using System.Linq;
using HarmonyLib;
using Project_Cobalt.Models;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCResourceManager : MonoBehaviour, IObjectComponent
    {
        public void Awake()
        {
            PCLogManager.LogLoaded("Splitter Manager Loaded");
        }
    }

    [HarmonyPatch(typeof(SDG.Unturned.ResourceManager), "damage")]
    class TellResourceDeadPatch
    {
        public static void Postfix(SDG.Unturned.ResourceManager __instance, Transform resource, Vector3 direction,
            float damage, float times, float drop, ref EPlayerKill kill, ref uint xp, CSteamID instigatorSteamID,
            EDamageOrigin damageOrigin, bool trackKill)
        {
            var steamPlayer = PlayerTool.getSteamPlayer(instigatorSteamID);
            
            if (Vector3.Distance(steamPlayer.player.transform.position, resource.position) > 3.3 ||
                damageOrigin != EDamageOrigin.Useable_Melee)
            {
                return;
            }

            var amount = (ushort) (damage * times);
            if (!SDG.Unturned.ResourceManager.tryGetRegion(resource, out var x, out var y, out var index)) return;
            var spawnpoint = SDG.Unturned.ResourceManager.getResourceSpawnpoint(x, y, index);
            var amountToAdd = (byte) (spawnpoint.isDead ? 40 : 20);
            var item = new Item(23100, amountToAdd, 100);
            var asset = (ItemAsset) Assets.find(EAssetType.ITEM, item.id);
            var searches = steamPlayer.player.inventory.search(item.id, true, true);
            if (searches.Count == 0)
            {
                steamPlayer.player.inventory.forceAddItem(item, true);
                return;
            }
            
            // Implement logic to give resource based on damage done to object, and bonus for destroying it

            foreach (var inventorySearch in searches.Where(inventorySearch =>
                inventorySearch.jar.item.amount < asset.amount))
            {
                var difference = asset.amount - inventorySearch.jar.item.amount;
                if (item.amount <= difference)
                {
                    steamPlayer.player.inventory.sendUpdateAmount(inventorySearch.page, inventorySearch.jar.x,
                        inventorySearch.jar.y, (byte) (inventorySearch.jar.item.amount + item.amount));
                    return;
                }

                steamPlayer.player.inventory.sendUpdateAmount(inventorySearch.page, inventorySearch.jar.x,
                    inventorySearch.jar.y, asset.amount);
                item.amount -= (byte) difference;
                steamPlayer.player.inventory.forceAddItem(item, true);
                return;
            }

            steamPlayer.player.inventory.forceAddItem(item, true);

            var percentage = Math.Round((float) amount / spawnpoint.asset.health);
        }
    }
}