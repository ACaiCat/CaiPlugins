using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using SBPlugin;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Ping
{
    [ApiVersion(2, 1)]
    public class Ping : TerrariaPlugin
    {
        public static ThreadStart threadStart = new ThreadStart(GetPing);
        private Thread thread = new Thread(threadStart);

        public override string Author => "Cai";

        public override string Description => "延迟测试";

        public override string Name => "Ping";

        public override Version Version => new Version(1, 0, 0, 0);

        public Ping(Main game)
        : base(game)
        {
            base.Order = int.MinValue;
        }
        public static bool threadOpen { get; set; } = true;
        public static string[] pings { get; set; } = new string[256];
        public static int[] pingValues { get; set; } = new int[256];
        public static string GetPing(TSPlayer plr)
        {
            return pings[plr.Index];
        }
        public override void Initialize()
        {
            //Commands.ChatCommands.Add(new Command("", Status, "status", "状态", "ping", "tps", "fps"));
           // ServerApi.Hooks.ServerChat.Register(this, OnChat,int.MaxValue);
            threadOpen = true;
            OTAPI.Hooks.MessageBuffer.GetData += PingClass.Hook_Ping_GetData;
            thread.Start();
            //ServerApi.Hooks.NpcStrike.Register(this, OnNPCStrike);
        }

        private void OnChat(ServerChatEventArgs args)
        {
            if (!args.Text.StartsWith(TShock.Config.Settings.CommandSilentSpecifier) && !args.Text.StartsWith(TShock.Config.Settings.CommandSpecifier))
            {
                var tSPlayer = TShock.Players[args.Who];
                string ChatText = "";
                ChatText = string.Format(TShock.Config.Settings.ChatFormat, tSPlayer.Group.Name, "[" + GetPing(tSPlayer) + "]" + tSPlayer.Group.Prefix, tSPlayer.Name, tSPlayer.Group.Suffix, args.Text);
                TSPlayer.Server.SendMessage(ChatText, Microsoft.Xna.Framework.Color.White);
                TSPlayer.All.SendMessage(ChatText, Microsoft.Xna.Framework.Color.White);
                TShock.Log.Write(ChatText, TraceLevel.Info);
                args.Handled = true;
            }
        }

        private void Status(CommandArgs args)
        {
            //分别发送全服玩家的延迟
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("当前服务器状态:");
            sb.AppendLine("[i:17]平均延迟:" + Ping.GetAveragePing());
            sb.AppendLine("[i:3099]服务器帧率(波动):" + ServerFPS.GetServerFPS());
            //发送全服玩家的延迟
            sb.AppendLine("[i:267]全服玩家延迟:");
            foreach (var i in TShock.Players)
            {
                if (i != null)
                {
                    sb.AppendLine($"[{GetPing(i)}]{i.Name}");
                }
            }
            args.Player.SendInfoMessage(sb.ToString());


        }

        //private void OnNPCStrike(NpcStrikeEventArgs args)
        //{
        //    Console.WriteLine($"{args.Npc.FullName}:{args.Npc.boss}");
        //}

        public static int GetAveragePingValue()
        {
            int ping = 0;
            foreach (var p in TShock.Players)
            {
                if (p == null || !p.Active)
                {
                    continue;
                }
                ping+= pingValues[p.Index];
            }
            return ping / TShock.Utils.GetActivePlayerCount();
        }
        public static string GetAveragePing()
        {
            return PingText(GetAveragePingValue());
        }
        public static async void SetPing(TSPlayer plr)
        {
            pingValues[plr.Index] = await PingClass.Command_Ping(plr);
            pings[plr.Index] = PingText(pingValues[plr.Index]);
        }

        public static string PingText(int ping)
        {
            if (ping == -1)
            {

                return "[c/FF0000:不可用]";
            }

            if (ping >= 200)
            {
                return $"[c/FF0000:{ping}ms]";

            }
            else if (ping > 80 && ping < 200)
            {
                return $"[c/FFA500:{ping}ms]";
            }
            else
            {
                return $"[c/00FF00:{ping}ms]";
            }
        }


    public static void GetPing()
    {
        try
        {
            while (threadOpen)
            {
                foreach (var plr in TShock.Players)
                {
                    if (plr != null && plr.Active)
                    {


                        SetPing(plr);

                    }
                }
                Thread.Sleep(1000);
            }
        }
        catch (Exception EX)
        {
            Console.WriteLine(EX.ToString());
        }
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
