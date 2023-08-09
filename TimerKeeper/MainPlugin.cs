﻿using System.Drawing;
using System.IO.Streams;
using System.Text;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace TimerKeeper
{
    [ApiVersion(2, 1)]
    public class TimerKeeper : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "保存计时器";

        public override string Name => "TimerKeeper";

        public override Version Version => new Version(1, 0, 0, 0);
        public static PlayerData data { get; set; }
        public TimerKeeper(Main game)
        : base(game)
        {
        }
        public override void Initialize()
        {
            DB.Connect();
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            //当世界加载完成
            GetDataHandlers.TileEdit.Register(OnTileEdit);
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInitialize);
        }

        private void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs e)
        {
            if (Main.tile[e.X, e.Y].type == 144)
            {
                DB.RemoveTimer(e.X, e.Y);
            }
        }

        private void OnPostInitialize(EventArgs args)
        {
            var timers = DB.LoadAll();
            foreach (var i in timers)
            {
                if (!WorldGen.InWorld(i.X, i.Y) || Main.tile[i.X, i.Y] == null || Main.tile[i.X, i.Y].type != 144)
                {
                    DB.RemoveTimer(i.X, i.Y);
                    continue;
                }
                else
                {
                    Main.tile[i.X, i.Y].frameY = 18;
                    Wiring.CheckMech(i.X, i.Y, 18000);
                }
            }
            TShock.Log.ConsoleWarn("[TimerKeeper]计时器已经加载!");
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.HitSwitch)
            {
                using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
                {
                    int i = data.ReadInt16();
                    int j = data.ReadInt16();
                    if (!WorldGen.InWorld(i, j) || Main.tile[i, j] == null || Main.tile[i, j].type != 144)
                    {
                        return;
                    }
                    if (Main.tile[i, j].frameY == 0)
                    {
                        DB.AddTimer(i, j);
                    }
                    else
                    {
                        DB.RemoveTimer(i, j);
                    }
                }


            }
        }




        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInitialize);
                GetDataHandlers.TileEdit.UnRegister(OnTileEdit);

            }
            base.Dispose(disposing);
        }


    }
}
