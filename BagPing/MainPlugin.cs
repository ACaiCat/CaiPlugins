using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using OTAPI;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.NetModules;
using Terraria.Net;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using static Terraria.Map.PingMapLayer;

namespace BagPing
{
    [ApiVersion(2, 1)]
    public class BagPing : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "BagPing";

        public override string Name => "BagPing";
   
        public override Version Version => new Version(1, 0, 0, 0);

        public BagPing(Main game)
        : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
        }


        private void OnNpcKilled(NpcKilledEventArgs e)
        {
            if (e.npc.boss)
            {
                TSPlayer.All.SendSuccessMessage(TShock.Utils.ItemTag(new Item() { netID = 3318, stack = 1, prefix = 0 })
                    + $"宝藏袋掉落在坐标({(int)e.npc.position.X/16},{(int)e.npc.position.Y/16}),已在小地图上标记!");
                Task.Run(() =>
                {                  
                    for (int i = 0; i < 4; i++)
                    {
                        foreach (var player in TShock.Players)
                        {
                            if (player != null && player.Active)
                            {
                                NetManager.Instance.SendToClient(NetPingModule.Serialize(new Vector2(e.npc.position.X/16, e.npc.position.Y/16)), player.Index);
                            }
                        }
                        Task.Delay(15000).Wait();
                    }
                });
            }
        }


       

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);

            }
            base.Dispose(disposing);
        }


    }
}
