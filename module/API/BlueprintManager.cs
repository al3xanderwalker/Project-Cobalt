using System.Collections.Generic;
using Project_Cobalt.API;
using Project_Cobalt.Models;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCBlueprintManager : MonoBehaviour, IObjectComponent
    {
        private static readonly List<CSteamID> Toggled = new List<CSteamID>();

        private static readonly List<ushort> BlueprintIds = new List<ushort>()
            {23001, 23002, 23003, 23004, 23005, 23006, 23007, 23008};

        public void Awake()
        {
            PCLogManager.LogLoaded("Blueprint Manager Loaded");

            PlayerInput.onPluginKeyTick += OnPluginKeyTick;
            EffectManager.onEffectButtonClicked += OnEffectButtonClicked;
            StructureManager.onDeployStructureRequested += OnDeployStructure;
        }

        private static void OnPluginKeyTick(Player player, uint simulation, byte key, bool state)
        {
            if (key != 0) return;
            if (player.equipment.asset == null ||
                !BlueprintIds.Contains(player.equipment.asset.id))
            {
                ToggleUI(player, false);
                return;
            }
            ToggleUI(player, state);
        }

        private static void OnDeployStructure(Structure structure, ItemStructureAsset asset, ref Vector3 point,
            ref float angleX, ref float angleY, ref float angleZ, ref ulong owner, ref ulong group,
            ref bool shouldAllow)
        {
            if (!PlayerTool.tryGetSteamPlayer(owner.ToString(), out var steamPlayer)) return;
            if (steamPlayer is null) return;

            // Insert Placement Cost Logic Here

            shouldAllow = true;
            var c = new Item(asset.id, true);
            steamPlayer.player.inventory.forceAddItem(c, true);
        }

        public void OnEffectButtonClicked(Player player, string buttonName)
        {
            if (buttonName.Substring(0, 8) != "BuildUI_") return;
            if (player.equipment.asset == null || !BlueprintIds.Contains(player.equipment.asset.id)) return;
            var id = 23000 + int.Parse(buttonName.Substring(8, 1));
            SwapBlueprint(player, (ushort) id);
        }


        public void SwapBlueprint(Player player, ushort id)
        {
            var inventory = player.inventory;
            for (byte page = 0; page < 8; page++)
            {
                var amountOfItems = inventory.getItemCount(page);
                for (var index = amountOfItems - 1; index >= 0; index--)
                {
                    var item = inventory.getItem(page, (byte) index);
                    if (!player.equipment.checkSelection(page, item.x, item.y)) continue;
                    player.inventory.removeItem(page, (byte) index);
                    player.inventory.items[page]
                        .addItem(item.x, item.y, item.rot, new Item(id, true));
                    player.equipment.tryEquip(page,
                        item.x,
                        item.y);
                }
            }
        }

        private static void ToggleUI(Player player, bool state)
        {
            var steamID = player.channel.owner.playerID.steamID;
            if (Toggled.Contains(steamID) && !state)
            {
                Toggled.Remove(steamID);
                EffectManager.askEffectClearByID(4642, steamID);
            }
            else if (!Toggled.Contains(steamID) && state)
            {
                Toggled.Add(steamID);
                EffectManager.sendUIEffect(4642, 4642, steamID, true);
            }   
            player.setPluginWidgetFlag(EPluginWidgetFlags.Modal, state);
            player.setPluginWidgetFlag(EPluginWidgetFlags.ForceBlur, state);
        }
    }
}