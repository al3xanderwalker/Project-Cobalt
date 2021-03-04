using Project_Cobalt.Commands;
using Project_Cobalt.Models;
using SDG.Unturned;
using UnityEngine;

namespace Project_Cobalt.API
{
    public class PCCommandManager : MonoBehaviour, IObjectComponent
    {
        public void Awake()
        {
            PCLogManager.LogLoaded("Command Manager Loaded");
            ChatManager.onCheckPermissions += OnCheckPermissions;
            
            Commander.register(new CommandTest());
            Commander.register(new CommandTp());
        }

        public void OnCheckPermissions(SteamPlayer steamPlayer, string text, ref bool shouldExecuteCommand,
            ref bool shouldList)
        {
            /*
            if (text.StartsWith("/discord") || text.StartsWith("/link"))
            {
                shouldExecuteCommand = true;
            }
            */
        }
    }
}