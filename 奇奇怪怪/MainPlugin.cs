using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.DataStructures;
using IL.Terraria.ID;
using IL.Terraria.Chat.Commands;
using Microsoft.Xna.Framework;
using System.Timers;
using On.OTAPI;
using TrProtocol;
using TrProtocol.Packets;
using static MonoMod.InlineRT.MonoModRule;

namespace Test
{
    [ApiVersion(2, 1)]
    public class Test : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "测试";

        public override string Name => "测试";
        internal static List<TSPlayer> Viewer = new(); //幽灵飞舞
        internal static System.Timers.Timer? BuffUpdate;
        internal static int[] FwBuffs = { 199, 10, 12, 35, 23 }; //去抄一下企鹅服,没抄到,伤心
        public override Version Version => new Version(1, 0, 0, 0);

        public Test(Main game)
        : base(game)
        {
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("test", TestCmd, "test"));
            On.OTAPI.Hooks.MessageBuffer.InvokeGetData += OnGetData;
            //BuffUpdate = new System.Timers.Timer { Interval = 1000, AutoReset = true, Enabled = true };
            //BuffUpdate.Elapsed += OnBuffUpdate;
        }

        private bool OnGetData(Hooks.MessageBuffer.orig_InvokeGetData orig, MessageBuffer instance, ref byte packetId, ref int readOffset, ref int start, ref int length, ref int messageType, int maxPackets)
        {
            if (messageType == 150)
            {
                instance.ResetReader();
                instance.reader.BaseStream.Position = start + 1;
                var PlayerSlot = instance.reader.ReadByte();
                var Platform = instance.reader.ReadByte();
   
                var PlayerId = instance.reader.ReadString();
                Console.WriteLine($"[PE]PlayerSlot={PlayerSlot},Plat={Platform},ID={PlayerId}");
            }
            return orig(instance, ref packetId, ref readOffset, ref start, ref length, ref messageType, maxPackets);
        }



        //private void OnBuffUpdate(object? sender, ElapsedEventArgs e)
        //{
        //    foreach (var i in Viewer)
        //    {
        //        if (i != null && i.Active)
        //        {
        //            foreach (var buff in FwBuffs)
        //            {
        //                i.SetBuff(buff, 600);
        //            }
        //        }
        //    }
        //}
        private void TestCmd(CommandArgs args)
        {

            PacketSerializer serializer = new(false);
            //添加一个假玩家
            var p = new SyncPlayer();
            p.Name = "测试";
            p.PlayerSlot = 254;
            p.Bit5 = 1;
            p.Bit4 = 1;
            p.Bit3 = 1;
            p.Bit2 = 1;
            p.Bit1 = 1;
                p.Hair = 1;
            p.HairColor = TrProtocol.Models.Color.White;
            p.HairDye = 1;
            p.HideMisc = 1;
            p.PantsColor = TrProtocol.Models.Color.White;
            p.ShirtColor = TrProtocol.Models.Color.White;
               p.ShoeColor = TrProtocol.Models.Color.White;
            p.SkinColor = TrProtocol.Models.Color.White;
            p.SkinVariant = 1;
            
            var data = serializer.Serialize(p);
            args.Player.SendRawData(data);
            var p2 = new SpawnPlayer();
            p2.PlayerSlot = 254;
            p2.Position = new TrProtocol.Models.ShortPosition(0, 0);

            data = serializer.Serialize(p2);
            args.Player.SendRawData(data);
            Task.Run(() =>
            {
                for(; ; )
                {
                    Task.Delay(1000).Wait();
                    var p3 = new PlayerControls();
                    p3.PlayerSlot = 254;
                    p3.Position = new TrProtocol.Models.Vector2(Random.Shared.Next(1, 100000), Random.Shared.Next(1, 100000));
                    args.Player.SendRawData(serializer.Serialize(p3));
                }

            });
            
        }




        public static Vector2[] GetSymmetricPoints(NPC npc, Player plr)
        {
            if (npc.position == plr.position)
            {
                return new Vector2[3] { npc.position, npc.position, npc.position };
            }

            Vector2[] point = new Vector2[3];
            point[0] = new Vector2(plr.position.X * 2 - npc.position.X, npc.position.Y);
            point[1] = new Vector2(plr.position.X, plr.position.Y * 2 - npc.position.Y);
            point[2] = new Vector2(point[0].X, point[1].Y);
            return point;
        }





        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }


    }
}
