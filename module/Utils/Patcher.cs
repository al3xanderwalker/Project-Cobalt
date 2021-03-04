using HarmonyLib;

namespace Project_Cobalt.Utils
{
    public class Patcher
    {
        public static void DoPatching()
        {
            var harmony = new Harmony("com.project-cobalt.patch");
            harmony.PatchAll();
        }
    }
}