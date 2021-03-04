using System;
using System.Collections.Generic;
using System.Linq;
using Project_Cobalt.Models;
using Project_Cobalt.Utils;
using rust_unturned_module.API.Junkpiles;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Project_Cobalt.API
{
    public class PCJunkpileManager : MonoBehaviour, IObjectComponent
    {
        public static List<Junkpile> Junkpiles = new List<Junkpile>();
        public bool ServerLoaded;
        public static List<Road> Roads = new List<Road>();
        public static int MaxJunkpiles;

        public void Awake()
        {
            PCLogManager.LogLoaded("Junkpile Manager Loaded");

            SDG.Unturned.Level.onPostLevelLoaded += (int level) =>
            {
                ServerLoaded = true;
                var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                LoadRoads();
                BarricadeManager.askClearAllBarricades();
                ItemManager.askClearAllItems();
                var totalRoad = 0f;
                Roads.ForEach(road => totalRoad += road.getLengthEstimate());
                MaxJunkpiles = (int) Math.Round(totalRoad / SDG.Unturned.Level.size) * 40;
                for (var i = 0; i < MaxJunkpiles; i++)
                {
                    if (!TryGenerateRoadLocation(out var randomPosition)) i = MaxJunkpiles;
                    SpawnJunkpile(randomPosition);
                }

                var timeFinish = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                
                PCLogManager.Log(
                    $"\nRoads: {Roads.Count} - Junkpiles: {Junkpiles.Count} - Time: {timeFinish - time}ms - Map: {Provider.map}", ConsoleColor.White);
            };
            BarricadeManager.onBarricadeSpawned += OnBarricadeSpawned;
            BarricadeManager.onDamageBarricadeRequested += OnDamageBarricadeRequest;
        }

        public void OnBarricadeSpawned(BarricadeRegion region, BarricadeDrop drop)
        {
            if (drop.asset.id != 1241 || !ServerLoaded) return;

            SpawnJunkpile(drop.model.position);
        }

        private static void OnDamageBarricadeRequest(CSteamID instigatorSteamID, Transform barricadeTransform,
            ref ushort pendingTotalDamage, ref bool shouldAllow, EDamageOrigin damageOrigin)
        {
            if (!BarricadeManager.tryGetInfo(barricadeTransform, out var x, out var y, out var num, out var index,
                out var barricadeRegion)) return;
            if (barricadeRegion.barricades[(int) index].barricade.health > pendingTotalDamage) return;

            var targetJunkpile = Junkpiles.FirstOrDefault(junkpile =>
            {
                return junkpile.Lootables.FirstOrDefault(lootable =>
                    lootable.Transform.GetInstanceID() == barricadeTransform.GetInstanceID()) != null;
            });
            if (targetJunkpile is null) return;
            targetJunkpile.Lootables.Remove(targetJunkpile.Lootables.FirstOrDefault(lootable =>
                lootable.Transform.GetInstanceID() == barricadeTransform.GetInstanceID()));

            if (targetJunkpile.Lootables.Count != 0) return;
            targetJunkpile.Despawn();
            Junkpiles.Remove(targetJunkpile);
        }

        private static void LoadRoads()
        {
            for (var i = 0; i < 1000; i++)
            {
                var road = LevelRoads.getRoad(i);
                if (road == null) i = 1000;
                else Roads.Add(road);
            }
        }

        private static bool TryGenerateRoadLocation(out Vector3 randomPoint)
        {
            randomPoint = Vector3.zero;
            for (var i = 0; i < 100 && randomPoint == Vector3.zero; i++)
            {
                var tmpPoint = GenerateRoadLocation();
                tmpPoint.x += Random.Range(-2f, 2f);
                tmpPoint.z += Random.Range(-2f, 2f);
                if (!Junkpiles.Any(junkpile => Vector3.Distance(junkpile.JunkTransform.position, tmpPoint) < 10f))
                    randomPoint = tmpPoint;
            }

            if (randomPoint == Vector3.zero) return false;
            randomPoint.y = LevelGround.getHeight(randomPoint);
            return true;
        }

        private static Vector3 GenerateRoadLocation()
        {
            var randomRoads = new Dictionary<Road, int>();
            Roads.ForEach(road => randomRoads.Add(road, (int) Math.Round(road.getLengthEstimate())));
            var randomRoad = WeightedRandomizer.From(randomRoads).TakeOne();
            var randomJointIndex = Random.Range(0, randomRoad.joints.Count - 2);
            var point1 = randomRoad.joints[randomJointIndex].vertex;
            var point2 = randomRoad.joints[randomJointIndex + 1].vertex;
            var randomPoint = Vector3.Lerp(point1, point2, Random.Range(0, 1f));
            return randomPoint;
        }

        public static void SpawnJunkpile(Vector3 position)
        {
            Junkpiles.Add(new Junkpile(position));
        }
    }
}