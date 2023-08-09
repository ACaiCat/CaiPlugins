using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using MonoMod.Utils;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Platform
{
    [ApiVersion(2, 1)]
    public class Platform : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "玩家设备API";

        public override string Name => "Platform";

        public override Version Version => new Version(1, 0, 0, 0);

        public Platform(Main game)
        : base(game)
        {
            base.Order = int.MinValue;
        }
        public static PlatformType[] Platforms { get; set; } = new PlatformType[256];
        public static string GetPlatform(TSPlayer plr)
        {
            return Platforms[plr.Index].ToString();
        }
        public override void Initialize()
        {
            On.OTAPI.Hooks.MessageBuffer.InvokeGetData += OnGetData;
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
        }

        private void OnJoin(JoinEventArgs args)
        {
            TShock.Log.ConsoleInfo($"[Platform]玩家{TShock.Players[args.Who].Name}游玩平台:{Platforms[args.Who]}");
        }

        public enum PlatformType : byte // TypeDefIndex: 5205
        {
            PE = 0,
            Stadia = 1,
            XBOX = 2,
            PSN = 3,
            Editor = 4,
            Switch = 5,
            PC = 233
        }


        private bool OnGetData(On.OTAPI.Hooks.MessageBuffer.orig_InvokeGetData orig, MessageBuffer instance, ref byte packetId, ref int readOffset, ref int start, ref int length, ref int messageType, int maxPackets)
        {
            if (messageType == 1)
            {
                Platforms[instance.whoAmI] = PlatformType.PC;
            }

            if (messageType == 150)
            {
                instance.ResetReader();
                instance.reader.BaseStream.Position = start + 1;
                var PlayerSlot = instance.reader.ReadByte();
                var Platform = instance.reader.ReadByte();
                Platforms[PlayerSlot] = (PlatformType)Platform;
                Console.WriteLine($"[PE]PlayerSlot={PlayerSlot},Plat={Platform}");
            }
            return orig(instance, ref packetId, ref readOffset, ref start, ref length, ref messageType, maxPackets);
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
