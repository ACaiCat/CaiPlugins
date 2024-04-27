using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace 自定义武器
{
    [ApiVersion(2, 1)]
    public class 自定义武器 : TerrariaPlugin
    {
        public override string Name => "自定义武器";

        public override string Author => "Johuan 汉化修改：Alex 羽学 Cai改";

        public override string Description => "允许您生成自定义物品";

        public override Version Version => new Version(1, 3, 0, 0);

        public 自定义武器(Main game)
            : base(game)
        {
        }

        public Config config = new Config();
        public override void Initialize()
        {
            config = Config.Read("tshock/自定义武器.json");
            GeneralHooks.ReloadEvent += GeneralHooks_ReloadEvent;
            Commands.ChatCommands.Add(new Command("自定", ACmd, "允许强化", "aqh")  //权限还没写!@!!!!@##@#
            {
            });

            Commands.ChatCommands.Add(new Command("自定", Cmd, "自定", "zd")  //权限还没写!@!!!!@##@#
            {
                HelpText = "[c/FFCCFF:/自定] <物品ID> <参数> <数据> \n参数: [c/C9FCE1: 伤害, 击退, 动画, 时间, 射速, 比例] "
            });
            Commands.ChatCommands.Add(new Command(QhCmd, "强化手持", "qh")
            {
                HelpText = "[c/FFCCFF:/强化] <参数> <数据> \n参数: [c/C9FCE1: 伤害, 击退, 动画, 时间, 射速, 比例] "
            });
            Commands.ChatCommands.Add(new Command("自定", HyCmd, "还原强化", "hy")
            {
                HelpText = "[c/FFCCFF:/还原强化] <参数> <数据> \n参数: [c/C9FCE1: 伤害, 击退, 动画, 时间, 射速, 比例] "
            });
            Commands.ChatCommands.Add(new Command("强化修改", XgCmd, "强化修改", "qghg")
            {
                HelpText = "[c/FFCCFF:/强化修改] <最大/最小+参数> <数据> \n参数: [c/C9FCE1: 伤害, 击退, 动画, 时间, 射速, 比例] "
            });

        }

        private void XgCmd(CommandArgs args)
        {
            if (args.Parameters.Count() < 2)
            {
                args.Player.SendErrorMessage("[i:398][c/E65AD8:用法错误：][c/FFCCFF:/强化修改 <最大/最小+参数> <数字>] ... \n[c/E65AD8:参数：][c/E2FFCC: 伤害, 击退, 动画, 时间, 射速, 比例] \n[c/E65AD8:例如：][c/E1E459:/qhxg zdsh 100 zxsj 1] = [c/E1E459:/强化手持 最大伤害 100 最小时间 1]");
                return;
            }
            if (args.Player.SelectedItem.maxStack != 1)
            {
                args.Player.SendWarningMessage("[i:398]请手持一个工具或者武器哦！");
                return;
            }
            try
            {
                for (int i = 0; i < args.Parameters.Count(); i++)
                {
                    switch (args.Parameters[i])
                    {
                        case "最大伤害":
                        case "zdsh":
                            config.Damage.max = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]伤害最大阈值已设为[c/97dd33:{config.Damage.max}]");
                            continue;
                        case "最小伤害":
                        case "zxsh":
                            config.Damage.min = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]伤害最小阈值已设为[c/97dd33:{config.Damage.min}]");
                            continue;
                        case "最大击退":
                        case "zdjt":
                            config.KnockBack.max = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]击退最大阈值已设为[c/97dd33:{config.KnockBack.max}]");
                            continue;
                        case "最小击退":
                        case "zxjt":
                            config.KnockBack.min = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]击退最小阈值已设为[c/97dd33:{config.KnockBack.min}]");
                            continue;
                        case "最大动画":
                        case "zddh":
                            config.Animation.max = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]动画最大阈值已设为[c/97dd33:{config.Animation.max}]");
                            continue;
                        case "最小动画":
                        case "zxdh":
                            config.Animation.min = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]动画最小阈值已设为[c/97dd33:{config.Animation.min}]");
                            continue;
                        case "最大时间":
                        case "zdsj":
                            config.UseTime.max = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]时间最大阈值已设为[c/97dd33:{config.UseTime.max}]");
                            continue;
                        case "最小时间":
                        case "zxsj":
                            config.UseTime.min = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]时间最小阈值已设为[c/97dd33:{config.UseTime.min}]");
                            continue;
                        case "最大射速":
                        case "zdss":
                            config.ShootSpeed.max = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]射速最大阈值已设为[c/97dd33:{config.ShootSpeed.max}]");
                            continue;
                        case "最小射速":
                        case "zxss":
                            config.ShootSpeed.min = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]射速最小阈值已设为[c/97dd33:{config.ShootSpeed.min}]");
                            continue;
                        case "最大比例":
                        case "zdbl":
                            config.Scale.max = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]比例最大阈值已设为[c/97dd33:{config.Scale.max}]");
                            continue;
                        case "最小比例":
                        case "zxbl":
                            config.Scale.min = int.Parse(args.Parameters[i + 1]);
                            args.Player.SendWarningMessage($"[i:398]比例最小阈值已设为[c/97dd33:{config.Scale.min}]");
                            continue;
                    }
                  
                }
                config.Write("tshock/自定义武器.json");
            }
            catch
            {
                args.Player.SendErrorMessage("[c/E65AD8:用法错误：][c/FFCCFF:/强化修改 <最大/最小+参数> <数字>] ... \n[c/E65AD8:参数：][c/E2FFCC: 伤害, 击退, 动画, 时间, 射速, 比例] \n[c/E65AD8:例如：][c/E1E459:/qhxg zdsh 100 zxsj 1] = [c/E1E459:/强化手持 最大伤害 100 最小时间 1]");
                return;
            }
        }

        private void GeneralHooks_ReloadEvent(ReloadEventArgs e)
        {
            config = Config.Read("tshock/自定义武器.json");
            TShock.Log.ConsoleInfo("[自定义武器]配置文件重读完毕!");
        }

        public Dictionary<string, DateTime> allowQh = new Dictionary<string, DateTime>();
        private void ACmd(CommandArgs args)
        {
            if (allowQh.ContainsKey(args.Player.Name))
            {
                allowQh[args.Player.Name] = DateTime.Now;
            }
            else
            {
                allowQh.Add(args.Player.Name, DateTime.Now);
            }
            args.Player.SendSuccessMessage("[i:398]你现在可以使用/强化手持(/qh)哦!");
        }

        public void Qhhy(TSPlayer plr, int item)
        {
            for (int i = 0; i < plr.TPlayer.inventory.Length; i++)
            {
                if (plr.TPlayer.inventory[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, i, plr.TPlayer.inventory[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.armor.Length; i++)
            {
                if (plr.TPlayer.armor[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Armor0 + i, plr.TPlayer.armor[i].prefix);
                }
            }
            for (int i = 0; i < plr.TPlayer.dye.Length; i++)
            {
                if (plr.TPlayer.dye[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Dye0 + i, plr.TPlayer.dye[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.miscEquips.Length; i++)
            {
                if (plr.TPlayer.miscEquips[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Misc0 + i, plr.TPlayer.miscEquips[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.miscDyes.Length; i++)
            {
                if (plr.TPlayer.miscDyes[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.MiscDye0 + i, plr.TPlayer.miscDyes[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank.item.Length; i++)
            {
                if (plr.TPlayer.bank.item[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank1_0 + i, plr.TPlayer.bank.item[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank2.item.Length; i++)
            {
                if (plr.TPlayer.bank2.item[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank2_0 + i, plr.TPlayer.bank2.item[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank3.item.Length; i++)
            {
                if (plr.TPlayer.bank3.item[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank3_0 + i, plr.TPlayer.bank3.item[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.bank4.item.Length; i++)
            {
                if (plr.TPlayer.bank4.item[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Bank4_0 + i, plr.TPlayer.bank4.item[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[0].Armor.Length; i++)
            {
                if (plr.TPlayer.Loadouts[0].Armor[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout1_Armor_0 + i, plr.TPlayer.Loadouts[0].Armor[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[1].Armor.Length; i++)
            {
                if (plr.TPlayer.Loadouts[1].Armor[i].netID == item)
                {
                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout2_Armor_0 + i, plr.TPlayer.Loadouts[1].Armor[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[2].Armor.Length; i++)
            {
                if (plr.TPlayer.Loadouts[2].Armor[i].netID == item)
                {

                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout3_Armor_0 + i, plr.TPlayer.Loadouts[2].Armor[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[0].Dye.Length; i++)
            {
                if (plr.TPlayer.Loadouts[0].Dye[i].netID == item)
                {

                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout1_Dye_0 + i, plr.TPlayer.Loadouts[0].Dye[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[1].Dye.Length; i++)
            {
                if (plr.TPlayer.Loadouts[1].Dye[i].netID == item)
                {

                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout2_Dye_0 + i, plr.TPlayer.Loadouts[1].Dye[i].prefix);

                }
            }
            for (int i = 0; i < plr.TPlayer.Loadouts[2].Dye.Length; i++)
            {
                if (plr.TPlayer.Loadouts[2].Dye[i].netID == item)
                {

                    NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.Loadout3_Dye_0 + i, plr.TPlayer.Loadouts[2].Dye[i].prefix);

                }
            }
            if (plr.TPlayer.trashItem.netID == item)
            {

                NetMessage.SendData(5, -1, -1, null, plr.Index, PlayerItemSlotID.TrashItem, plr.TPlayer.trashItem.prefix);
            }
        }
        private void HyCmd(CommandArgs args)
        {
            Qhhy(args.Player, int.Parse(args.Parameters[0]));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= GeneralHooks_ReloadEvent;
            }
            base.Dispose(disposing);
        }

        private void QhCmd(CommandArgs args)
        {
            if (!allowQh.ContainsKey(args.Player.Name) || (DateTime.Now - allowQh[args.Player.Name]).TotalSeconds > config.aQhSeconds)
            {
                args.Player.SendErrorMessage("[i:398]你现在无法使用强化哦!");
                return;
            }

            if (args.Parameters.Count() < 2)
            {
                args.Player.SendErrorMessage("[i:398][c/E65AD8:请输入：][c/FFCCFF:/强化手持 <参数> <数字>] ... \n[c/E65AD8:参数：][c/E2FFCC: 伤害, 击退, 动画, 时间, 射速, 比例] \n[c/E65AD8:例如：][c/E1E459:/qh sh 100 sj 1] = [c/E1E459:/强化手持 伤害 100 时间 1]");
                return;
            }
            if (args.Player.SelectedItem.maxStack != 1)
            {
                args.Player.SendErrorMessage("[i:398]请选择一个工具或者武器哦！");
                return;
            }
            Item item = TShock.Utils.GetItemById(args.Player.SelectedItem.netID);
            TSPlayer tSPlayer = new TSPlayer(args.Player.Index);
            int num = Item.NewItem(new EntitySource_DebugCommand(), (int)tSPlayer.X, (int)tSPlayer.Y, item.width, item.height, item.type, item.maxStack);
            Item item2 = Main.item[num];
            item2.playerIndexTheItemIsReservedFor = args.Player.Index;
            item2.prefix = args.Player.SelectedItem.prefix;
            try
            {
                for (int i = 0; i < args.Parameters.Count(); i++)
                {
                    switch (args.Parameters[i])
                    {
                        case "伤害":
                        case "sh":
                            item2.damage = int.Parse(args.Parameters[i + 1]);
                            if (item2.damage > config.Damage.max && config.Damage.max != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]伤害超出服务器阈值({config.Damage.min}~{config.Damage.max})");
                                return;
                            }
                            if (item2.damage < config.Damage.min && config.Damage.min != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]伤害低于服务器阈值({config.Damage.min}~{config.Damage.max})");
                                return;
                            }
                            i++;
                            continue;
                        case "击退":
                        case "jt":
                            item2.knockBack = float.Parse(args.Parameters[i + 1]);
                            if (item2.knockBack > config.KnockBack.max && config.KnockBack.max != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]击退超出服务器阈值({config.KnockBack.min}~{config.KnockBack.max})");
                                return;
                            }
                            if (item2.knockBack < config.KnockBack.min && config.KnockBack.min != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]击退低于服务器阈值({config.KnockBack.min}~{config.KnockBack.max})");
                                return;
                            }
                            i++;
                            continue;
                        case "动画":
                        case "dh":
                            item2.useAnimation = int.Parse(args.Parameters[i + 1]);
                            if (item2.useAnimation > config.Animation.max && config.Animation.max != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]动画超出服务器阈值({config.Animation.min}~{config.Animation.max})");
                                return;
                            }
                            if (item2.useAnimation < config.Animation.min && config.Animation.min != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]动画低于服务器阈值({config.Animation.min}~{config.Animation.max})");
                                return;
                            }
                            i++;
                            continue;
                        case "时间":
                        case "sj":
                        case "time":
                            item2.useTime = int.Parse(args.Parameters[i + 1]);
                            if (item2.useTime > config.UseTime.max && config.UseTime.max != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]时间超出服务器阈值({config.UseTime.min}~{config.UseTime.max})");
                                return;
                            }
                            if (item2.useTime < config.UseTime.min && config.UseTime.min != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]时间低于服务器阈值({config.UseTime.min}~{config.UseTime.max})");
                                return;
                            }
                            i++;
                            continue;
                        case "射速":
                        case "ss":
                            item2.shootSpeed = float.Parse(args.Parameters[i + 1]);
                            if (item2.shootSpeed > config.ShootSpeed.max && config.ShootSpeed.max != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]射速超出服务器阈值({config.ShootSpeed.min}~{config.ShootSpeed.max})");
                                return;
                            }
                            if (item2.shootSpeed < config.ShootSpeed.min && config.ShootSpeed.min != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]射速低于服务器阈值({config.ShootSpeed.min}~{config.ShootSpeed.max})");
                                return;
                            }
                            i++;
                            continue;
                        case "比例":
                        case "bl":
                            item2.scale = float.Parse(args.Parameters[i + 1]);
                            if (item2.scale > config.Scale.max && config.Scale.max != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]比例超出服务器阈值({config.Scale.min}~{config.Scale.max})");
                                return;
                            }
                            if (item2.scale < config.Scale.min && config.Scale.min != -1)
                            {
                                args.Player.SendWarningMessage($"[i:398]比例低于服务器阈值({config.Scale.min}~{config.Scale.max})");
                                return;
                            }
                            i++;
                            continue;
                    }
                }
            }
            catch
            {
                args.Player.SendErrorMessage("[i:398][c/E65AD8:用法错误：][c/FFCCFF:/强化手持 <参数> <数字>] ... \n[c/E65AD8:参数：][c/E2FFCC: 伤害, 击退, 动画, 时间, 射速, 比例] \n[c/E65AD8:例如：][c/E1E459:/qh sh 100 sj 1] = [c/E1E459:/强化手持 伤害 100 时间 1]");
                return;
            }

            if (args.TPlayer.inventory[args.TPlayer.selectedItem].netID != args.Player.SelectedItem.netID && args.TPlayer.inventory[args.TPlayer.selectedItem].prefix != args.Player.SelectedItem.prefix)
            {
                args.Player.SendErrorMessage("[i:398]未知错误！");
                return;

            }
            args.TPlayer.inventory[args.TPlayer.selectedItem].SetDefaults(0);
            NetMessage.SendData(5, -1, -1, null, args.Player.Index, args.TPlayer.selectedItem);
            TSPlayer.All.SendData(PacketTypes.PlayerSlot, null, num);
            TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, null, num);
            TSPlayer.All.SendData(PacketTypes.ItemOwner, null, num);
            TSPlayer.All.SendData(PacketTypes.TweakItem, null, num, 255f, 63f);
            args.Player.SendSuccessMessage("[i:398]强化成功！");
            Task.Run(delegate ()
            {
                Thread.Sleep(config.qhSeconds * 1000);
                if (args.Player == null)
                {
                    return;
                }
                Qhhy(args.Player, args.Player.SelectedItem.netID);
                args.Player.SendWarningMessage("[i:398]你的武器强化效果已经被移除了哦！");
            });
        }
        private void Cmd(CommandArgs args)
        {
            List<string> parameters = args.Parameters;
            if (parameters.Count() == 0)
            {
                args.Player.SendErrorMessage("[i:398][c/E65AD8:请输入：][c/FFCCFF:/自定 <物品ID> <参数> <数字>] ... \n[c/E65AD8:参数：][c/E2FFCC: 伤害, 击退, 动画, 时间, 射速, 比例] \n[c/E65AD8:例如：][c/E1E459:/zd 164 sh 100 sj 1] = [c/E1E459:/自定 手枪 伤害 100 时间 1]");
                return;
            }
            List<Item> itemByIdOrName = TShock.Utils.GetItemByIdOrName(args.Parameters[0]);
            if (itemByIdOrName.Count == 0)
            {
                args.Player.SendErrorMessage("[i:398]未找到 " + args.Parameters[0] + " 的ID或参数");
                return;
            }
            Item item = itemByIdOrName[0];
            TSPlayer tSPlayer = new TSPlayer(args.Player.Index);
            int num = Item.NewItem(new EntitySource_DebugCommand(), (int)tSPlayer.X, (int)tSPlayer.Y, item.width, item.height, item.type, item.maxStack);
            Item item2 = Main.item[num];
            item2.playerIndexTheItemIsReservedFor = args.Player.Index;
            try
            {
                for (int i = 1; i < parameters.Count(); i++)
                {
                    switch (parameters[i])
                    {
                        case "伤害":
                        case "sh":
                            item2.damage = int.Parse(args.Parameters[i + 1]);
                            i++;
                            continue;
                        case "击退":
                        case "jt":
                            item2.knockBack = float.Parse(args.Parameters[i + 1]);
                            i++;
                            continue;
                        case "动画":
                        case "dh":
                            item2.useAnimation = int.Parse(args.Parameters[i + 1]);
                            i++;
                            continue;
                        case "时间":
                        case "sj":
                        case "time":
                            item2.useTime = int.Parse(args.Parameters[i + 1]);
                            i++;
                            continue;
                        case "射速":
                        case "ss":
                            item2.shootSpeed = float.Parse(args.Parameters[i + 1]);
                            i++;
                            continue;
                        case "比例":
                        case "bl":
                            item2.scale = float.Parse(args.Parameters[i + 1]);
                            i++;
                            continue;
                    }
                }
            }
            catch
            {
                args.Player.SendErrorMessage("[i:398][c/E65AD8:用法错误：][c/FFCCFF:/自定 <物品ID> <参数> <数字>] ... \n[c/E65AD8:参数：][c/E2FFCC: 伤害, 击退, 动画, 时间, 射速, 比例] \n[c/E65AD8:例如：][c/E1E459:/zd 164 sh 100 sj 1] = [c/E1E459:/自定 手枪 伤害 100 时间 1]");
                return;
            }


            TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, null, num);
            TSPlayer.All.SendData(PacketTypes.ItemOwner, null, num);
            TSPlayer.All.SendData(PacketTypes.TweakItem, null, num, 255f, 63f);
        }
    }
}


