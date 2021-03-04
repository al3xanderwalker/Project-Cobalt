using System.Linq;
using System.Reflection;
using Project_Cobalt.Models;
using Project_Cobalt.Utils;
using SDG.Framework.Modules;
using SDG.Unturned;
using UnityEngine;
using ModuleConfig = Project_Cobalt.API.ModuleConfig;

namespace Project_Cobalt
{
    public class Main : MonoBehaviour, IModuleNexus
    {
        public static MainConfig Config;
        public static BuildCostConfig BuildCostConfig;

        private GameObject _pureVanillaObject;

        public void initialize()
        {
            CommandWindow.LogError($"Loaded Project Cobalt");

            UnityThread.initUnityThread();

            var patch = new Patcher();
            Patcher.DoPatching();

            ModuleConfig.LoadConfigs();

            _pureVanillaObject = new GameObject();
            var componentManagers = Assembly.GetExecutingAssembly().DefinedTypes
                .Where(type => type.ImplementedInterfaces.Any(inter => inter == typeof(IObjectComponent))).ToList();
            componentManagers.ForEach(c =>
            {
                var methodInfo = typeof(GameObject)
                    .GetMethods()
                    .Where(x => x.IsGenericMethod).Single(x => x.Name == "AddComponent");
                var addComponentRef = methodInfo.MakeGenericMethod(c);
                addComponentRef.Invoke(_pureVanillaObject, null);
            });
        }

        public void shutdown()
        {
            CommandWindow.Log($"Unloaded Project Cobalt");
            Destroy(_pureVanillaObject);
            _pureVanillaObject = null;
        }
    }
}