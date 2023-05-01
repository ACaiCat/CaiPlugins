using Chireiden.TShock.Omni;
using MySqlX.XDevAPI.Common;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using StatusTxtMgr;
using System.Net.NetworkInformation;

namespace GameStatus
{
    [ApiVersion(2, 1)]
    public class GameStatus : TerrariaPlugin
    {
        public static ThreadStart threadStart = new ThreadStart(GetPing);

        public static string[] pings = new string[255];


        Thread thread = new Thread(threadStart);

        public override string Author => "Cai";

        public override string Description => "游戏用计分板";

        public override string Name => "游戏用计分板";

        public override Version Version => new Version(1, 0, 0, 0);

        public GameStatus(Main game)
        : base(game)
        {
            base.Order = int.MinValue;
        }
        public static bool threadOpen { get; set; } = true;

        public override void Initialize()
        {
            threadOpen = true;
            StatusTxtMgr.StatusTxtMgr.Hooks.StatusTextUpdate.Register(delegate (StatusTextUpdateEventArgs args)
            {
                var tsplayer = args.tsplayer;
                var statusTextBuilder = args.statusTextBuilder;
                statusTextBuilder.AppendLine($"[c/66CCFF:「][c/55d284:喵][c/62d27a:窝][c/6fd16f:服][c/7cd165:务][c/89d15a:器][c/66CCFF:」]");
                statusTextBuilder.AppendLine($"[i:267]在线人数:[c/00FF00:{TShock.Utils.GetActivePlayerCount()}]/{TShock.Config.Settings.MaxSlots}");
                statusTextBuilder.AppendLine($"[i:306]当前组:[c/00BFFF:{tsplayer.Group.Name}]");
                statusTextBuilder.AppendLine($"[i:3122]Ping(延迟):{pings[tsplayer.Index]}");
            }, 60uL);
            OTAPI.Hooks.MessageBuffer.GetData += PingClass.Hook_Ping_GetData;
            thread.Start();
        }

        public static async void GetPing()
        {

            while (threadOpen)
            {
                foreach (var plr in TShock.Players)
                {
                    if (plr != null && plr.RealPlayer && plr.Active)
                    {
                        try
                        {
                            pings[plr.Index] = await PingClass.Command_Ping(plr);
                        }
                        catch
                        {
                            
                        }
                    }
                }
                Thread.Sleep(1000);
            }
            return;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                OTAPI.Hooks.MessageBuffer.GetData -= PingClass.Hook_Ping_GetData;
                threadOpen = false;
            }
            base.Dispose(disposing);
        }


    }
}
