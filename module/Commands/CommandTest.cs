using SDG.Unturned;
using Steamworks;

namespace Project_Cobalt.Commands
{
    public class CommandTest : Command
    {
        protected override void execute(CSteamID executorID, string parameter)
        {
            //CommandWindow.Log(SplitterManager.SplitData1.Count);   
            var steamPlayer = PlayerTool.getSteamPlayer(executorID);

            /*
            if (parameter.Length == 0) parameter = "0 0 0 0";
            var list = parameter.Split(' ').ToList().ConvertAll(float.Parse);
            while (list.Count != 4) list.Add(0f);
            */
            var pos = steamPlayer.player.transform.position;
            pos.x += 10f;
            BarricadeManager.dropBarricade(
                new Barricade(46440), null,
                pos, 0f, 0f, 0f, 0UL, 0UL);
        }
        
        public CommandTest()
        {
            localization = new Local();
            _command = "test";
            _info = "test";
            _help = "test";
        }
    }
}