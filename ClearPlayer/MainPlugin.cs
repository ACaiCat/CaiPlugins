using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Terraria.DataStructures;
using IL.Terraria.ID;
using IL.Terraria.Chat.Commands;

namespace GUAI
{
    [ApiVersion(2, 1)]
    public class GUAI : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "锤击祭坛爆神装";

        public override string Name => "锤击祭坛爆神装";

        public override Version Version => new Version(1, 0, 0, 0);

        public GUAI(Main game)
        : base(game)
        {
        }

        public List<int> goodItem = new();
        public override void Initialize()
        {
            GetDataHandlers.TileEdit.Register(OnTileEdit);
            Commands.ChatCommands.Add(new Command("", blockpos, "blockpos"));
        }

        private void blockpos(CommandArgs args)
        {
            Main.tile[args.Player.TileX, args.Player.TileY].ClearTile();
            //发包更新
            NetMessage.SendTileSquare(-1, args.Player.TileX, args.Player.TileY, 1);


        }

        private void On()
        {
            for (int i = 0; i < 6000; i++)
            {
                Item n = new();
                n.SetDefaults(i);
                n.SetDefaults1(i);
                if (n.damage > 80)
                {
                    goodItem.Add(i);
                }
            }
        }
        //记录祭坛挖掘次数int
        Dictionary<(int,int),int> blockCount = new();
        int[] goodBuff = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 28, 29, 30, 40, 58, 59, 60, 61, 170, 171, 172, 173, 174, 175, 176, 177, 178, 179, 180 };


        private void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs e)
        {

            if (Main.tile[e.X, e.Y].type == 26)
            {
                if (!blockCount.ContainsKey((e.X, e.Y)))
                {
                    blockCount.Add((e.X, e.Y), 0);
                }
                //当祭坛挖掘10次破坏祭坛
                if (blockCount[(e.X, e.Y)] >= 10)
                {
                    Main.tile[e.X, e.Y].ClearTile();
                    Main.tile[e.X, e.Y].active(false);
                    Main.tile[e.X, e.Y].type = 0;
                    //发包更新
                    NetMessage.SendData(17, -1, -1, null, 0, e.X, e.Y, 0, 0, 0, 0);
                    blockCount.Add((e.X, e.Y), 0);
                    //发包更新
                    //循环10次
                    for (int i = 0; i < 100; i++)
                    {
                        



                        //从goodItem中随机一个物品ID

                        int itemID = goodItem[Main.rand.Next(0, goodItem.Count)];
                        int x = e.X + Main.rand.Next(-3, 3);

                        int y = e.Y + Main.rand.Next(0, 3);
                        //生成随机物品ID
                        int num = Item.NewItem(new EntitySource_DebugCommand(), (int)x * 16, (int)y * 16, 3, 3, itemID, 1, noBroadcast: true, 0, noGrabDelay: false);
                        e.Player.SendData(PacketTypes.ItemDrop, "", num, 1f);
                        e.Player.SendData(PacketTypes.ItemOwner, null, num);

                    }
                }
                blockCount[(e.X, e.Y)]++;

                if (goodItem.Count == 0)
                {
                    On();

                }
                //随机给增益goodbuff
                int buff = goodBuff[Main.rand.Next(0, goodBuff.Length)];
                //随机持续时间 >= 1分钟
                int time = Main.rand.Next(3600, 72000);
                //给玩家增益
                e.Player.SetBuff(buff, time);
                //从goodItem中随机一个物品ID
               

                var life =  Main.rand.Next(73, 200);
                e.Player.TPlayer.statLifeMax += life;
                //发包更新
                NetMessage.SendData(16, -1, -1, null, e.Player.Index, 0, 0, 0, 0, 0, 0);
                e.Player.Heal(life);
                //循环10次
                for (int i= 0; i <1; i++)
                {

                    //从goodItem中随机一个物品ID

                        int itemID = goodItem[Main.rand.Next(0, goodItem.Count)];
                        int x = e.X + Main.rand.Next(-3, 3);

                        int y = e.Y + Main.rand.Next(0, 3);
                        //生成随机物品ID
                        int num = Item.NewItem(new EntitySource_DebugCommand(), (int)x*16, (int)y*16, 3 , 3, itemID, 1, noBroadcast: true, 0, noGrabDelay: true);
                        e.Player.SendData(PacketTypes.ItemDrop, "", num, 1f);
                        e.Player.SendData(PacketTypes.ItemOwner, null, num);

                }


            }
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
