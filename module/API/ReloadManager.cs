using HarmonyLib;
using Project_Cobalt.Models;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCReloadManager : MonoBehaviour, IObjectComponent
    {
        public void Awake()
        {
            PCLogManager.LogLoaded("Reload Manager Loaded");
        }
    }
    
    [HarmonyPatch(typeof(UseableGun), "askAttachMagazine")]
    internal class ReloadPatch
    {
        public static bool Prefix(UseableGun __instance, CSteamID steamID, byte page, byte x, byte y, byte[] hash)
        {
            var steamPlayer = __instance.channel.owner;
            var searches = steamPlayer.player.inventory.search(EItemType.MAGAZINE, __instance.equippedGunAsset.magazineCalibers);
            if (searches.Count == 0) return false;
            
            var magAsset = (ItemAsset) Assets.find(EAssetType.ITEM, __instance.equippedGunAsset.getMagazineID());

            var totalFound = steamPlayer.player.equipment.state[10];
            var initial = totalFound;
            searches.ForEach(search =>
            {
                var required = (byte) (magAsset.amount - totalFound);
                if (required == 0) return;
                if (search.jar.item.amount <= required)
                {
                    totalFound += search.jar.item.amount;
                    steamPlayer.player.inventory.removeItem(search.page, steamPlayer.player.inventory.getIndex(search.page,search.jar.x,search.jar.y));
                }
                else
                {
                    totalFound += required;
                    steamPlayer.player.inventory.sendUpdateAmount(search.page, search.jar.x, search.jar.y,
                        (byte) (search.jar.item.amount - required));
                }
            });
            if (initial == totalFound) return false;
            steamPlayer.player.equipment.state[10] = totalFound;
            steamPlayer.player.animator.channel.send("askReload", ESteamCall.ALL, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                new object[]
                {
                    steamPlayer.playerID.steamID,
                    false
                });
            steamPlayer.player.equipment.sendUpdateState();
            return false;
        }
    }
}