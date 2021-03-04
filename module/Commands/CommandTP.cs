using System.Linq;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace Project_Cobalt.Commands
{
    public class CommandTp: Command
    {
        protected override void execute(CSteamID executorID, string parameter)
        {
            var steamPlayer = PlayerTool.getSteamPlayer(executorID);

            if (parameter.Length == 0) parameter = "0 0 0";
            var list = parameter.Split(' ').ToList().ConvertAll(float.Parse);
            while (list.Count != 3) list.Add(0f);
            
            steamPlayer.player.teleportToLocationUnsafe(new Vector3(list[0], list[1], list[2]), 0);
        }

        public CommandTp()
        {
            localization = new Local();
            _command = "tp";
            _info = "tp";
            _help = "tp";
        }
    }
}