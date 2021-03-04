using Project_Cobalt.Models;
using SDG.Unturned;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCLevelManager : MonoBehaviour, IObjectComponent
    {
        private void Awake()
        {
            PCLogManager.LogLoaded("Level Manager Loaded");
            Provider.configData.Easy.Objects.Resource_Drops_Multiplier = 0;
            Provider.configData.Normal.Objects.Resource_Drops_Multiplier = 0;
            Provider.configData.Hard.Objects.Resource_Drops_Multiplier = 0;
        }
    }
}