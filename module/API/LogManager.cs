using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using Project_Cobalt.Models;
using SDG.Unturned;
using UnityEngine;
#pragma warning disable 4014

namespace Project_Cobalt.API
{
    public class PCLogManager : MonoBehaviour, IObjectComponent
    {
        public static bool Enabled = true;
        public static Dictionary<string, LogData> Data = new Dictionary<string, LogData>();

        public void Awake()
        {
            CommandWindow.LogError("Log Manager Loaded");
            Level.onPostLevelLoaded += (level) =>
            {
                Enabled = false;
                Log(@"   ___           _           _
  / _ \_ __ ___ (_) ___  ___| |_
 / /_)| '__/ _ \| |/ _ \/ __| __|
/ ___/| | | (_) | |  __| (__| |_
\_    |_|  \____/ |\___|\___|\__|
   ___      _ |__/      _ _
  / __\___ | |__   __ _| | |_
 / /  / _ \| '_ \ / _` | | __|
/ /__| (_) | |_) | (_| | | |_
\____/\___/|_.__/ \__,_|_|\__|
   __                 _          _
  / /  ___   __ _  __| | ___  __| |
 / /  / _ \ / _` |/ _` |/ _ \/ _` |
/ /__| (_) | (_| | (_| |  __| (_| |
\____/\___/ \__,_|\__,_|\___|\__,_|", ConsoleColor.Blue);
            };
            LogDashboard();
        }

        private static async Task LogDashboard()
        {
            Console.CursorVisible = false;
            while (Enabled)
            {
                Console.SetCursorPosition(0, 0);
                Console.WriteLine(new string(' ', 5000));
                Console.SetCursorPosition(0, 0);
                Log("Project Cobalt Loading", ConsoleColor.White);
                foreach (var key in Data.Keys)
                {
                    Log(Data[key].Value,Data[key].Color);
                }

                await Task.Delay(10);
            }
        }

        public static void Log(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ForegroundColor = ConsoleColor.White;
        }

        public static void LogLoaded(string value)
        {
            Data[value] = new LogData(value, ConsoleColor.Blue);
            CommandWindow.LogWarning(value);
        }
    }

    [HarmonyPatch(typeof(CommandWindow))]
    class CommandWindowPatches
    {
        [HarmonyPrefix]
        [HarmonyPatch("Log")]
        static bool Prefix(object text)
        {
            if (!PCLogManager.Enabled) return true;
            if (!(text is string)) return false;
            var phrases = new List<string>()
            {
                "Mounting Core:", "Preloading general assets:", "Preloading item assets:",
                "Preloading effect assets:", "Preloading object assets:", "Preloading vehicle assets:",
                "Preloading mythic assets:",
                "Preloading skin assets:", "Preloading spawn table assets:", "Preloading NPC assets:",
                "Loading level:"
            };
            var output = phrases.Find(x => text.ToString().ToUpper().Contains(x.ToUpper()));
            if (output != null) PCLogManager.Data[output] = new LogData(text.ToString(), ConsoleColor.White);

            return false;
        }
    }
}