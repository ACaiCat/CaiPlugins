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
        public static string[] pings { get; set; } = new string[255];
        public static string GetPing(TSPlayer plr)
        {
            return pings[plr.Index];
        }
        public override void Initialize()
        {
            threadOpen = true;
            OTAPI.Hooks.MessageBuffer.GetData += PingClass.Hook_Ping_GetData;
            thread.Start();
        }

        public static async void SetPing(TSPlayer plr)
        {
            pings[plr.Index] = await PingClass.Command_Ping(plr);
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
