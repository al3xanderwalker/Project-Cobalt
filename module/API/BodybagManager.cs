using Project_Cobalt.Commands;
using Project_Cobalt.Models;
using SDG.Unturned;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCBodybagManager : MonoBehaviour, IObjectComponent
    {
        public void Awake()
        {
            PCLogManager.LogLoaded("Bodybag Manager Loaded");
        }
    }

    [HarmonyPatch(typeof(PlayerLife))]
    class DoDamagePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("doDamage")]
        static void Prefix(PlayerLife __instance, byte amount, Vector3 newRagdoll, EDeathCause newCause, ELimb newLimb, CSteamID newKiller, ref EPlayerKill kill, bool trackKill, ERagdollEffect newRagdollEffect, bool canCauseBleeding)
        {
            var player = __instance.channel.owner.player;
            if (player is null) return;

            if (amount >= player.life.health)
            {
                var deathboxTransform = BarricadeManager.dropBarricade(new Barricade(1283),player.transform, player.transform.position, 0f, 0f, 0f, null, 0UL);

                if (!BarricadeManager.tryGetInfo(deathboxTransform, out var x, out var y, out var plant, out var index, out var region)) return;

                var storage = deathboxTransform.GetComponent<InteractableStorage>();

                storage.items.resize(10, 10); // do some calculations to find smallest size possible.

                // possibly have an specified row for clothing
                // transfer all items to the storage, clear players inventory.
                // Add some logic so that the bodybag has a set life time, increased relative to the items in it?

                for (byte page = 0; page < 6; page++)
                {
                    for (byte i = 0; i < ply.inventory.items[page].getItemCount(); i++)
                    {
                        ItemJar item = ply.inventory.items[page].getItem(i);
                        storage.items.tryAddItem(item.item);
                    }
                }
            }
        }
    }
}