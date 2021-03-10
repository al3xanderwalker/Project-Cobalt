using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using Project_Cobalt.Models;
using Project_Cobalt.Utils;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

#pragma warning disable 4014

namespace Project_Cobalt.API
{
    public class PCSplitterManager : MonoBehaviour, IObjectComponent
    {
        private static readonly Dictionary<SteamPlayer, SplitInfo> SplitData =
            new Dictionary<SteamPlayer, SplitInfo>();

        public void Awake()
        {
            PCLogManager.LogLoaded("SplitterManager Loaded");
            Provider.onEnemyConnected +=
                steamPlayer => SplitData[steamPlayer] = new SplitInfo(null, EStage.NotSplitting);
        }

        public static void InventoryOpened(SteamPlayer steamPlayer)
        {
            if (steamPlayer.player.inventory.isStoring) return;
            var inventory = new Items(7);
            inventory.resize(12, 2);
            inventory.onItemAdded += (page, index, jar) => OnItemAdded(steamPlayer, inventory, page, index, jar);
            inventory.onItemRemoved += (page, index, jar) => OnItemRemoved(steamPlayer, inventory, page, index, jar);
            inventory.onItemDiscarded += (page, index, jar) => OnItemRemoved(steamPlayer, inventory, page, index, jar);
            steamPlayer.player.inventory.items[2].resize(12, 10);
            steamPlayer.player.inventory.updateItems(7, inventory);
            steamPlayer.player.inventory.sendStorage();
        }

        private static void OnItemAdded(SteamPlayer steamPlayer, Items items, byte page, byte index,
            ItemJar itemJar)
        {
            var data = SplitData[steamPlayer];

            if (data.Stage == EStage.Splitting) GetSlot(steamPlayer, itemJar).Clear = false;

            if (data.Stage != EStage.NotSplitting) return;

            SplitItem(steamPlayer, items, itemJar.item);
        }

        private static void OnItemRemoved(SteamPlayer steamPlayer, Items items, byte page, byte index,
            ItemJar itemJar)
        {
            var data = SplitData[steamPlayer];
            if (data.Stage != EStage.Splitting) return;
            if (!GetSlot(steamPlayer, itemJar).Clear) return;

            GetSlot(steamPlayer, itemJar).Clear = false;
            if (itemJar.item.amount <= data.Item.amount) data.Item.amount -= itemJar.item.amount;
            if (data.Item.amount <= 0)
            {
                UnityThread.executeCoroutine(ClearItemsI(steamPlayer, items));
                SplitData[steamPlayer].Stage = EStage.SettingUp;
                DelayStageChange(steamPlayer);
            }
            else SplitItem(steamPlayer, items, data.Item);
        }

        private static async Task DelayStageChange(SteamPlayer steamPlayer)
        {
            await Task.Delay(1);
            SplitData[steamPlayer].Stage = EStage.NotSplitting;
        }

        private static async Task SplitItem(SteamPlayer steamPlayer, Items items, Item item)
        {
            SplitData[steamPlayer].Stage = EStage.SettingUp;
            items = steamPlayer.player.inventory.items[7];
            UnityThread.executeCoroutine(ClearItemsI(steamPlayer, items));
            await Task.Delay(50);
            UnityThread.executeCoroutine(SplitItemI(steamPlayer, items, item));
        }

        public static void AddItemStacked(SteamPlayer steamPlayer, ushort id, byte amount = 1, byte quality = 100,
            byte[] state = null)
        {
            var asset = (ItemAsset) Assets.find(EAssetType.ITEM, id);
            var item = new Item(id, amount, quality, state ?? new byte[0]);
            if (!steamPlayer.player.inventory.tryFindSpace(asset.size_x, asset.size_y, out var page, out var newX,
                out var newY, out var rot))
            {
                ItemManager.dropItem(item, steamPlayer.player.transform.position, false, true, true);
                return;
            }

            steamPlayer.player.inventory.tryAddItem(item, newX, newY, page, rot);
            var searches = steamPlayer.player.inventory.search(id, true, true);
            if (searches.Count == 0) return;
            foreach (var inventorySearch in searches.Where(inventorySearch =>
                inventorySearch.jar.item.amount < asset.amount))
            {
                if (inventorySearch.jar.x == newX && inventorySearch.jar.y == newY &&
                    inventorySearch.page == page) continue;
                StackItem(steamPlayer, inventorySearch.page, inventorySearch.jar.x,
                    inventorySearch.jar.y, page, newX, newY);
                break;
            }
        }

        public static void StackItem(SteamPlayer steamPlayer, byte page0, byte x0, byte y0, byte page1, byte x1,
            byte y1)
        {
            var inventory = steamPlayer.player.inventory;
            if (page0 == 7 || page1 == 7) return;
            var index0 = inventory.items[page0].getIndex(x0, y0);
            var item0 = inventory.items[page0].getItem(index0);
            var index1 = inventory.items[page1].getIndex(x1, y1);
            var item1 = inventory.items[page1].getItem(index1);
            if (item0.item.id != item1.item.id) return;
            var itemAsset = (ItemAsset) Assets.find(EAssetType.ITEM, item0.item.id);

            var possibleMoveAmount = (byte) (itemAsset.amount - item0.item.amount);
            if (possibleMoveAmount == 0) return;
            if (item1.item.amount <= possibleMoveAmount)
            {
                inventory.removeItem(page1, index1);
                inventory.sendUpdateAmount(page0, x0, y0, (byte) (item0.item.amount + item1.item.amount));
            }
            else
            {
                inventory.sendUpdateAmount(page1, x1, y1, (byte) (item0.item.amount + possibleMoveAmount));
                inventory.sendUpdateAmount(page0, x0, y0, (byte) (item1.item.amount - possibleMoveAmount));
            }
        }

        public static void ReturnItems(SteamPlayer steamPlayer)
        {
            if (SplitData[steamPlayer].Stage != EStage.Splitting) return;

            steamPlayer.player.inventory.items[7].items.ForEach(itemJar =>
            {
                if (!GetSlot(steamPlayer, itemJar).Clear)
                    AddItemStacked(steamPlayer, SplitData[steamPlayer].Item.id, SplitData[steamPlayer].Item.amount,
                        SplitData[steamPlayer].Item.quality,
                        SplitData[steamPlayer].Item.state);
            });
            AddItemStacked(steamPlayer, SplitData[steamPlayer].Item.id, SplitData[steamPlayer].Item.amount,
                SplitData[steamPlayer].Item.quality,
                SplitData[steamPlayer].Item.state);
            SplitData[steamPlayer] = new SplitInfo(null, EStage.NotSplitting);
        }

        private static IEnumerator SplitItemI(SteamPlayer steamPlayer, Items items, Item item)
        {
            var amountForItem = item.amount;
            var slots = new List<ItemSlot>();
            for (var i = 0; i < 6; i++)
            {
                if (amountForItem != 0 || i == 0)
                {
                    var tmpItem = new Item(item.id, amountForItem, item.quality);
                    items.tryAddItem(tmpItem);
                    slots.Add(new ItemSlot((byte) (i * 2), true));
                    amountForItem = (byte) Math.Floor(amountForItem / 2f);
                }
                else slots.Add(new ItemSlot((byte) (i * 2), false));
            }

            SplitData[steamPlayer] = new SplitInfo(item, EStage.Splitting, slots);
            yield return null;
        }

        private static IEnumerator ClearItemsI(SteamPlayer steamPlayer, Items items)
        {
            for (var i = items.getItemCount() - 1; i >= 0; i--)
            {
                var itemJar = items.getItem((byte) i);
                items.removeItem((byte) i);
                if (SplitData[steamPlayer].Slots.Count <= i) continue;
                if (GetSlot(steamPlayer, itemJar).Clear) continue;

                AddItemStacked(steamPlayer, itemJar.item.id, itemJar.item.amount, itemJar.item.quality,
                    itemJar.item.state);
            }

            SplitData[steamPlayer].Slots = new List<ItemSlot>();
            yield return null;
        }

        private static ItemSlot GetSlot(SteamPlayer steamPlayer, ItemJar itemJar)
        {
            return SplitData[steamPlayer].Slots.Find(slot => slot.XPos == itemJar.x) ?? new ItemSlot(itemJar.x, false);
        }
    }


    [HarmonyPatch(typeof(PlayerAnimator), "askGesture")]
    internal class AskGesturePatch
    {
        public static void Prefix(PlayerAnimator __instance, CSteamID steamID, byte id)
        {
            switch (id)
            {
                case 1:
                    PCSplitterManager.InventoryOpened(__instance.channel.owner);
                    break;
                case 2:
                    PCSplitterManager.ReturnItems(__instance.channel.owner);
                    break;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInventory), "askSwapItem")]
    internal class AskSwapItem
    {
        public static void Prefix(PlayerInventory __instance, CSteamID steamID, byte page_0, byte x_0, byte y_0,
            byte page_1, byte x_1, byte y_1)
        {
            PCSplitterManager.StackItem(__instance.channel.owner, page_0, x_0, y_0, page_1, x_1, y_1);
        }
    }

    [HarmonyPatch(typeof(ItemManager), "askTakeItem")]
    class TryAddItemPatch
    {
        public static bool Prefix(ItemManager __instance, CSteamID steamID, byte x, byte y, uint instanceID, byte to_x,
            byte to_y, byte to_rot, byte to_page)
        {
            if (!Regions.checkSafe(x, y)) return false;
            var player = PlayerTool.getPlayer(steamID);
            if (player == null) return false;
            if (player.life.isDead) return false;
            if (player.animator.gesture == EPlayerGesture.ARREST_START) return false;
            var steamPlayer = player.channel.owner;

            var itemRegion = ItemManager.regions[x, y];
            for (var i = 0; i < itemRegion.items.Count; i++)
            {
                var itemData = itemRegion.items[i];
                if (instanceID != itemData.instanceID) continue;
                if ((itemData.point - player.transform.position).sqrMagnitude > 400f) return false;
                var asset = (ItemAsset) Assets.find(EAssetType.ITEM, itemData.item.id);
                var searches = steamPlayer.player.inventory.search(itemData.item.id, true, true);
                if (searches.Count == 0) return true;
                foreach (var inventorySearch in searches.Where(inventorySearch =>
                    inventorySearch.jar.item.amount < asset.amount))
                {
                    var difference = asset.amount - inventorySearch.jar.item.amount;
                    if (itemData.item.amount <= difference)
                    {
                        player.inventory.sendUpdateAmount(inventorySearch.page, inventorySearch.jar.x,
                            inventorySearch.jar.y, (byte) (inventorySearch.jar.item.amount + itemData.item.amount));
                        ItemManager.regions[x, y].items.RemoveAt(i);
                        //ItemManager.takeItem(itemRegion.drops[i].model,0,0,0,0);
                        __instance.channel.send("tellTakeItem", ESteamCall.CLIENTS, x, y, ItemManager.ITEM_REGIONS,
                            ESteamPacket.UPDATE_RELIABLE_BUFFER, x, y, instanceID);
                            
                        return false;
                    }

                    player.inventory.sendUpdateAmount(inventorySearch.page, inventorySearch.jar.x,
                        inventorySearch.jar.y, asset.amount);
                    itemData.item.amount = (byte) (itemData.item.amount - difference);
                    ItemManager.regions[x, y].items.RemoveAt(i);
                    //ItemManager.takeItem(itemRegion.drops[i].model,0,0,0,0);
                    __instance.channel.send("tellTakeItem", ESteamCall.CLIENTS, x, y, ItemManager.ITEM_REGIONS,
                        ESteamPacket.UPDATE_RELIABLE_BUFFER, x, y, instanceID);
                        
                    ItemManager.dropItem(itemData.item, itemData.point, true, true, true);
                    return true;
                }

                return true;
            }

            return true;
        }
    }
}