using System;
using System.Collections.Generic;
using System.Linq;
using Project_Cobalt.Models;
using SDG.Unturned;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCCraftingManager : MonoBehaviour, IObjectComponent
    {
        public void Awake()
        {
            PCLogManager.LogLoaded("Crafting Manager Loaded");
            PlayerCrafting.onCraftBlueprintRequested += OnCraftBlueprintRequested;
        }

        private static void OnCraftBlueprintRequested(PlayerCrafting crafting, ref ushort id, ref byte index,
            ref bool allow)
        {
            var player = crafting.channel.owner.player;
            allow = false;

            var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, id);
            if (itemAsset is null) return;
            if (index >= itemAsset.blueprints.Count) return;
            var blueprint = itemAsset.blueprints[index];
            // Do a check to see if they are near a workbench or something lol
            if (!string.IsNullOrEmpty(blueprint.map) &&
                !blueprint.map.Equals(SDG.Unturned.Level.info.name, StringComparison.InvariantCultureIgnoreCase)) return;
            if (blueprint.tool != 0 && player.inventory.has(blueprint.tool) == null) return;

            var array = new List<InventorySearch>[blueprint.supplies.Length];

            for (var i = 0; i < blueprint.supplies.Length; i++)
            {
                var blueprintSupply = blueprint.supplies[i];
                var list = player.inventory.search(blueprintSupply.id, false, true);
                if (list.Count == 0) return;

                var totalCount =
                    list.Aggregate<InventorySearch, ushort>(0, (current, t) => (ushort) (current + t.jar.item.amount));
                if (totalCount < blueprintSupply.amount) return;

                list.Sort(new InventorySearchAmountAscendingComparator());
                array[i] = list;
            }


            for (var i = 0; i < array.Length; i++)
            {
                var blueprintSupply = blueprint.supplies[i];
                var searches = array[i];
                var totalFound = 0;
                searches.ForEach(search =>
                {
                    var required = (byte) (blueprintSupply.amount - totalFound);
                    if (required == 0) return;
                    if (search.jar.item.amount <= required)
                    {
                        totalFound += search.jar.item.amount;
                        crafting.removeItem(search.page, search.jar);
                    }
                    else
                    {
                        totalFound += required;
                        player.inventory.sendUpdateAmount(search.page, search.jar.x, search.jar.y,
                            (byte) (search.jar.item.amount - required));
                    }
                });
            }

            byte b13 = 0;
            while (b13 < blueprint.outputs.Length)
            {
                var blueprintOutput = blueprint.outputs[b13];
                var outputItemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, blueprintOutput.id);
                if (outputItemAsset.amount != 1)
                {
                    var totalOuput = 0;

                    while (totalOuput != blueprintOutput.amount)
                    {
                        var remainder = (byte) (blueprintOutput.amount - totalOuput);
                        if (outputItemAsset.amount <= remainder)
                        {
                            crafting.channel.owner.player.inventory.forceAddItem(
                                new Item(blueprintOutput.id, outputItemAsset.amount, 100), true);
                            totalOuput += outputItemAsset.amount;
                        }
                        else
                        {
                            crafting.channel.owner.player.inventory.forceAddItem(
                                new Item(blueprintOutput.id, remainder, 100), true);
                            totalOuput += remainder;
                        }
                    }
                }
                else
                {
                    crafting.channel.owner.player.inventory.forceAddItem(
                        blueprint.transferState
                            ? new Item(blueprintOutput.id, array[0][0].jar.item.amount, array[0][0].jar.item.quality,
                                array[0][0].jar.item.state)
                            : new Item(blueprintOutput.id, blueprintOutput.origin), true);
                }

                b13 += 1;
            }

            blueprint.applyConditions(crafting.channel.owner.player, true);
            blueprint.grantRewards(crafting.channel.owner.player, true);
            crafting.channel.send("tellCraft", ESteamCall.OWNER, ESteamPacket.UPDATE_RELIABLE_BUFFER,
                Array.Empty<object>());
        }
    }
}