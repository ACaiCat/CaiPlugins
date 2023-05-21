using System.Data;
using System.Reflection;
using System.Text;
using SSCManager;
using StatusTxtMgr;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace Parkour
{
    [ApiVersion(2, 1)]
    public class Parkour : TerrariaPlugin
    {
        public Parkour(Terraria.Main game) : base(game)
        {
        }

        public override string Name => "Parkour";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => "Cai";

        public override string Description => "Parkour";

        public List<ParkourPlay> parkourPlays { get; set; } = new List<ParkourPlay>();

        //缓存所以跑酷信息，并且在保存地图时插入数据库
        public List<ParkourInfo> parkourInfos { get; set; } = new List<ParkourInfo>();
        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            GeneralHooks.ReloadEvent += OnReload;
            //玩家进入区域后发送消息

            RegionHooks.RegionEntered += RegionHooks_RegionEntered;
            RegionHooks.RegionLeft += RegionHooks_RegionLeft;
            //GetDataHandlers.PlayerSlot.Register(OnPlayerSlot);
            ServerApi.Hooks.WorldSave.Register(this, OnSave);
            GetDataHandlers.PlayerSpawn.Register(OnPlayerSpawn);
            ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
            GetDataHandlers.KillMe.Register(OnKillMe);
            //有掉落物品事件
            // GetDataHandlers.ItemDrop.Register(OnItemDrop);
            //检测玩家敲击标牌
            GetDataHandlers.SignRead.Register(OnSignReadText);
            //添加计分板事件

            TableManager.CreateTables();
            parkourInfos = DB.GetAllParkour();
            //添加命令跑酷主命令
            Commands.ChatCommands.Add(new Command("parkour.admin", ParkourCommand, "parkour", "跑酷", "par"));
            Commands.ChatCommands.Add(new Command("", ParkourExit, "parkourExit", "跑酷退出", "退出跑酷"));

            //添加命令跑酷排行
            #region
            StatusTxtMgr.StatusTxtMgr.Hooks.StatusTextUpdate.Register(delegate (StatusTextUpdateEventArgs args)
            {
                var tsplayer = args.tsplayer;
                var statusTextBuilder = args.statusTextBuilder;
                statusTextBuilder.AppendLine();
                var p = parkourPlays.GetParkourByName(tsplayer.Name);
                //检测玩家在跑酷区内，不是跑酷中
                var par = parkourInfos.Where(x => x.Region.InArea(tsplayer.TileX, tsplayer.TileY)).FirstOrDefault();
                if (p == null && par != null)
                {
                    string reward = "";
                    if (!par.AwardCDRecords.ContainsKey(tsplayer.Account.ID) || DateTime.Now - par.AwardCDRecords[tsplayer.Account.ID] >= TimeSpan.FromHours(par.AwardCD))
                    {
                        reward = $"[c/FFD700:({par.Award}金币)]";
                    }
                    else
                    {
                        reward = $"[c/FF8C00:(奖励冷却中)]";
                    }
                    statusTextBuilder.AppendLine($"跑酷点[{par.Name}]:{reward}");
                    //按照p.Records的记录排序，要求可以显示键值
                    var records = new Dictionary<int, TimeSpan>(par.Records);
                    var recordsRank = records.OrderBy(x => x.Value.TotalSeconds).Take(5);
                    var rank = 1;
                    foreach (var record in recordsRank)
                    {
                        var acc = TShock.UserAccounts.GetUserAccountByID(record.Key);
                        if (acc == null)
                        {
                            continue;
                        }
                        switch (rank)
                        {
                            case 1:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:4601][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:4601]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");

                                }
                                break;
                            case 2:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:4600][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:4600]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");
                                }
                                break;
                            case 3:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:4599][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:4599]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");

                                }
                                break;
                            default:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:1067][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:1067]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");
                                }
                                break;

                        }
                        rank++;
                    }
                    if (par.Records.ContainsKey(tsplayer.Account.ID))
                    {
                        statusTextBuilder.AppendLine($"\n[i:3099][c/40E0D0:最高记录:][c/1E90FF:{Math.Round(par.Records[tsplayer.Account.ID].TotalSeconds, 2)}][c/40E0D0:秒]" +
                            $"\n[i:321][c/2F4F4F:死亡次数:][c/778899:不可用]");
                    }
                    else
                    {
                        statusTextBuilder.AppendLine($"\n[i:3099][c/40E0D0:最高记录:][c/1E90FF:你从未完成过该跑酷!]" +
                           $"\n[i:321][c/2F4F4F:死亡次数:][c/778899:不可用]");
                    }
                }

                if (p != null)
                {

                    string reward = "";
                    if (!p.parkour.AwardCDRecords.ContainsKey(tsplayer.Account.ID) || DateTime.Now - p.parkour.AwardCDRecords[tsplayer.Account.ID] >= TimeSpan.FromHours(p.parkour.AwardCD))
                    {
                        reward = $"[c/FFD700:({p.parkour.Award}金币)]";
                    }
                    else
                    {
                        reward = $"[c/FF8C00:(奖励冷却中)]";
                    }
                    statusTextBuilder.AppendLine($"跑酷点[{p.parkour.Name}]:{reward}");
                    //按照p.Records的记录排序，要求可以显示键值
                    var records = new Dictionary<int, TimeSpan>(p.parkour.Records);
                    if (records.ContainsKey(args.tsplayer.Account.ID))
                    {
                        records[args.tsplayer.Account.ID] = p.currentTime;
                    }
                    else
                    {
                        records.Add(args.tsplayer.Account.ID, p.currentTime);
                    }
                    var recordsRank = records.OrderBy(x => x.Value.TotalSeconds).Take(5);
                    var rank = 1;
                    foreach (var record in recordsRank)
                    {
                        var acc = TShock.UserAccounts.GetUserAccountByID(record.Key);
                        if (acc == null)
                        {
                            continue;
                        }
                        switch (rank)
                        {
                            case 1:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:4601][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:4601]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");

                                }
                                break;
                            case 2:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:4600][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:4600]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");
                                }
                                break;
                            case 3:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:4599][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:4599]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");

                                }
                                break;
                            default:
                                if (acc.Name == tsplayer.Name)
                                {
                                    statusTextBuilder.AppendLine($"[i:1067][c/00BFFF:{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒]");

                                }
                                else
                                {
                                    statusTextBuilder.AppendLine($"[i:1067]{acc.Name}-{Math.Round(record.Value.TotalSeconds, 2)}秒");
                                }
                                break;

                        }
                        rank++;
                    }
                    statusTextBuilder.AppendLine($"\n[i:3099][c/40E0D0:当前用时:][c/1E90FF:{p.GetTime}][c/40E0D0:秒]" +
                        $"\n[i:321][c/2F4F4F:死亡次数:][c/778899:{p.DeathTimes}][c/2F4F4F:次]");
                }
            });
            #endregion

        }


        private void ParkourExit(CommandArgs args)
        {
            //退出玩家进行的跑酷
            var tsplayer = args.Player;
            var p = parkourPlays.GetParkourByName(tsplayer.Name);
            if (p != null)
            {
                args.Player.Spawn(PlayerSpawnContext.RecallFromItem);
                parkourPlays.Remove(p);

                args.Player.SendSuccessMessage("[i:3099]你已退出跑酷!");
            }
            else
            {
                args.Player.SendErrorMessage("[i:3099]你当前没有在进行跑酷!");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
                GeneralHooks.ReloadEvent -= OnReload;
                RegionHooks.RegionEntered -= RegionHooks_RegionEntered;
                RegionHooks.RegionLeft -= RegionHooks_RegionLeft;
                GetDataHandlers.PlayerSlot.UnRegister(OnPlayerSlot);
                ServerApi.Hooks.WorldSave.Deregister(this, OnSave);
                GetDataHandlers.PlayerSpawn.UnRegister(OnPlayerSpawn);
                ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
                GetDataHandlers.KillMe.UnRegister(OnKillMe);
                GetDataHandlers.ItemDrop.UnRegister(OnItemDrop);
                GetDataHandlers.SignRead.UnRegister(OnSignReadText);
            }
            base.Dispose(disposing);
        }
        private void OnItemDrop(object sender, GetDataHandlers.ItemDropEventArgs e)
        {
            //检测掉落的物品是否在跑酷区内

            foreach (var par in parkourInfos)
            {
                if (par.Region.InArea((int)e.Position.X, (int)e.Position.Y))
                {
                    //清除这个掉落物
                    Main.item[e.ID].active = false;
                    e.Player.SendData(PacketTypes.ItemDrop, "", e.ID);
                    e.Handled = true;
                    return;
                }

            }

        }

        public static int i = 0;
        private void OnUpdate(EventArgs args)
        {
            if (i % 600 == 0 && i != 0)
            {
                foreach (var par in parkourPlays)
                {
                    par.player.SendWarningMessage($"[i:3099]跑酷已进行{par.GetTime}秒!");
                }
            }
            //写个定时器，每60*60刻执行一次、
            if (i == 3600)
            {
                i = 0;
                foreach (var p in parkourInfos)
                {
                    if (p.SignPos == null || p.SignPos.X == -1 || p.SignPos.Y == -1)
                    {
                        return;
                    }
                    var sb = new StringBuilder();
                    sb.AppendLine($"跑酷[{p.Name}]排行榜:({p.Award}金币)");
                    //按照p.Records的记录排序，要求可以显示键值
                    var records = p.Records.OrderBy(x => x.Value.TotalSeconds).Take(6);
                    var rank = 1;
                    foreach (var record in records)
                    {
                        var acc = TShock.UserAccounts.GetUserAccountByID(record.Key);
                        if (acc == null)
                        {
                            continue;
                        }
                        sb.AppendLine($"{rank}. {acc.Name} - {Math.Round(record.Value.TotalSeconds, 2)}秒");
                        rank++;
                    }
                    sb.Append($"\n*点击标牌查看你的记录!!!");
                    for (int i = 0; i < 1000; i++)
                    {
                        if (Main.sign[i] != null && Main.sign[i].x == p.SignPos.X && Main.sign[i].y == p.SignPos.Y)
                        {
                            try
                            {
                                Sign.TextSign(i, sb.ToString());

                            }
                            catch
                            {
                                TShock.Log.ConsoleError($"[跑酷]标牌写入失败!可能是标牌不存在!({p.SignPos.ToString()}");
                            }
                        }
                    }

                }
            }

            i++;

        }

        private void OnSignReadText(object sender, GetDataHandlers.SignReadEventArgs e)
        {
            var p = parkourInfos.FirstOrDefault(x => x.SignPos.X == e.X && x.SignPos.Y == e.Y);
            if (p != null)
            {
                var sb = new StringBuilder();
                sb.AppendLine($"跑酷[{p.Name}]排行榜:({p.Award}金币)");
                //按照p.Records的记录排序，要求可以显示键值
                var records = p.Records.OrderBy(x => x.Value.TotalSeconds).Take(6);
                var rank = 1;
                foreach (var record in records)
                {
                    var acc = TShock.UserAccounts.GetUserAccountByID(record.Key);
                    if (acc == null)
                    {
                        continue;
                    }
                    sb.AppendLine($"{rank}. {acc.Name} - {Math.Round(record.Value.TotalSeconds, 2)}秒");
                    rank++;
                }
                //添加自己的记录
                //找到玩家自己的记录

                if (p.Records.ContainsKey(e.Player.Account.ID))
                {
                    sb.Append($"\n你的记录:{Math.Round(p.Records[e.Player.Account.ID].TotalSeconds,2)}秒");
                }
                else
                {
                    sb.Append($"\n你没有完成过该跑酷哦!!!");

                }
                e.Handled = true;
                //发送数据包更新标牌内容
                try
                {
                    e.Player.SendRawData(new RawDataBuilder(PacketTypes.SignNew).PackInt16(0).PackInt16((short)e.X).PackInt16((short)e.Y).PackString(sb.ToString()).PackByte((byte)e.Player.Index).PackByte(new BitsByte(false)).GetByteData());
                }
                catch (Exception ex) { TShock.Log.Error(ex.Message); }
            }
            if (e.Player.GetData<string>("ParkourSign") != null)
            {
                //找出e.Player.GetData<string>("ParkourSign")对应的跑酷
                var par = parkourInfos.FirstOrDefault(x => x.Name == e.Player.GetData<string>("ParkourSign"));
                par.SignPos = new(e.X, e.Y);
                e.Player.SendSuccessMessage($"跑酷点{par.Name}标牌绑定成功!");//显示跑酷点名字
                e.Player.SetData<string>("ParkourSign", null);
            }

        }

        private void OnReload(ReloadEventArgs e)
        {
            parkourInfos = DB.GetAllParkour();
            e.Player.SendSuccessMessage("[Parkour]跑酷点信息已重载!");
        }

        private void OnLeave(LeaveEventArgs args)
        {
            var p = parkourPlays.GetParkourByName(TShock.Players[args.Who].Name);
            if (p != null)
            {
                parkourPlays.RemoveAll(x => x.player.Index == args.Who);

            }

        }

        private void OnPlayerSpawn(object sender, GetDataHandlers.SpawnEventArgs e)
        {
            var p = parkourPlays.GetParkourByName(e.Player.Name);
            if (p != null)
            {
                Task.Run(delegate
                {
                    Thread.Sleep(500);
                    e.Player.Teleport(p.SpawnPoint.X, p.SpawnPoint.Y);
                });
            }
        }

        private void OnKillMe(object sender, GetDataHandlers.KillMeEventArgs e)
        {

            var p = parkourPlays.GetParkourByName(e.Player.Name);
            if (p != null)
            {
                p.DeathTimes++;
                e.Player.SendInfoMessage($"{(p.DeathTimes >= 100 ? "[i:3230]" : "[i:1173]")}你已经挂了{p.DeathTimes}次");
                p.lastDeathTime = DateTime.Now;
                e.Player.Spawn(PlayerSpawnContext.ReviveFromDeath);
                p.player.RespawnTimer = -1;
                e.Handled = true;
            }

        }


        private void OnPlayerSlot(object sender, GetDataHandlers.PlayerSlotEventArgs e)
        {
            if (parkourInfos.Exists(x => x.Region == e.Player.CurrentRegion))
            {
                //发送数据包修改被修改的slot
                if (e.Player.HasPermission("parkour.build"))
                    return;
                e.Handled = true;

                if (e.Stack == e.Player.TPlayer.inventory[e.Slot].stack - 1)
                    return;
                e.Player.SendData(PacketTypes.PlayerSlot, "", e.Player.Index, e.Slot, e.Stack, e.Prefix);

            }

        }
        private void ParkourCommand(CommandArgs args)
        {
            var player = args.Player;
            if (args.Parameters.Count == 0)
            {
                player.SendInfoMessage($"[i:1311]跑酷命令列表:\n" +
                        $"/parkour add [c/FFD700:跑酷名字] - 创建跑酷\n" +
                        $"/parkour del [c/FFD700:跑酷名字] - 删除跑酷\n" +
                        $"/parkour start [c/FFD700:跑酷名字] - 开始跑酷\n" +
                        $"/parkour end - 结束跑酷\n" +
                        $"/parkour list - 查看跑酷列表\n" +
                        $"/parkour info [c/FFD700:跑酷名字] - 查看跑酷信息\n" +
                        $"/parkour setspawn [c/FFD700:跑酷名字] - 设置跑酷出生点\n" +
                        $"/parkour delrecode [c/FFD700:跑酷名字] [c/FFD700:玩家名] - 删除玩家的跑酷记录\n" +
                        $"/parkour clearcd [c/FFD700:跑酷点名称] [c/FFD700:玩家名] - 清除玩家的跑酷奖励CD\n" +
                        $"/parkour rank [c/FFD700:跑酷名字] [c/FFD700:页数] - 查看跑酷排行榜");
                return;
            }
            switch (args.Parameters[0].ToLower())
            {
                case "exit":
                    var p = parkourPlays.Find(x => x.player.Name == args.Player.Name);
                    if (p == null)
                    {
                        player.SendErrorMessage($"你没有进行的跑酷");
                        return;
                    }
                    parkourPlays.RemoveAll(x => x.player.Name == args.Player.Name);
                    player.SendErrorMessage($"你退出了当前进行的跑酷");
                    break;
                case "back":
                    var par = parkourPlays.Find(x => x.player.Name == args.Player.Name);
                    if (par == null)
                    {
                        player.SendErrorMessage($"你没有进行的跑酷");
                        return;
                    }
                    args.Player.Teleport(par.SpawnPoint.X, par.SpawnPoint.Y);
                    break;
                case "add":
                    if (args.Parameters.Count < 6)
                    {
                        args.Player.SendErrorMessage("用法: /parkour add [名称] [使用的背包ID] [Region区域名称] [奖励] [奖励冷却]");
                        return;
                    }
                    //检测参数可用
                    if (parkourInfos.Find(x => x.Name == args.Parameters[1]) != null)
                    {
                        player.SendErrorMessage($"跑酷:{args.Parameters[1]}已经存在");
                        return;
                    }
                    if (!SSCSaver.ExistBag(int.Parse(args.Parameters[2])))
                    {
                        player.SendErrorMessage($"背包ID:{args.Parameters[2]}不存在");
                        return;
                    }
                    if (TShock.Regions.GetRegionByName(args.Parameters[3]) == null)
                    {
                        player.SendErrorMessage($"区域:{args.Parameters[3]}不存在");
                        return;
                    }

                    var cmdInfo = new ParkourInfo();
                    cmdInfo.Name = args.Parameters[1];
                    cmdInfo.BagID = int.Parse(args.Parameters[2]);
                    cmdInfo.RegionName = args.Parameters[3];
                    cmdInfo.Award = int.Parse(args.Parameters[4]);
                    cmdInfo.AwardCD = int.Parse(args.Parameters[5]);
                    cmdInfo.AwardCDRecords = new Dictionary<int, DateTime>();
                    cmdInfo.Records = new Dictionary<int, TimeSpan>();
                    DB.AddParkour(cmdInfo);
                    parkourInfos = DB.GetAllParkour();
                    player.SendSuccessMessage($"你已经保存了跑酷:{cmdInfo.Name}");
                    player.SendInfoMessage($"跑酷名称:{cmdInfo.Name}");
                    player.SendInfoMessage($"跑酷背包ID:{cmdInfo.BagID}的SSC数据");
                    player.SendInfoMessage($"跑酷区域:{cmdInfo.Region.Name}");
                    player.SendInfoMessage($"跑酷奖励:{cmdInfo.Award}金币");
                    player.SendInfoMessage($"跑酷奖励冷却:{cmdInfo.AwardCD}小时");
                    break;
                case "del":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("用法: /parkour del [名称]");
                        return;
                    }
                    var delInfo = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (delInfo != null)
                    {
                        parkourInfos.Remove(delInfo);
                        player.SendSuccessMessage($"已经删除了跑酷:{delInfo.Name}");
                    }
                    else
                    {
                        player.SendErrorMessage($"跑酷:{delInfo.Name}不存在");
                    }
                    break;
                case "list":
                    foreach (var i in parkourInfos)
                    {
                        player.SendInfoMessage($"跑酷名称:{i.Name}");
                    }
                    break;
                case "info":
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("用法: /parkour info [跑酷点名称]");
                        return;
                    }
                    var info = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (info != null)
                    {
                        player.SendInfoMessage($"跑酷名称:{info.Name}");
                        player.SendInfoMessage($"跑酷背包ID:{info.BagID}号SSC数据");
                        player.SendInfoMessage($"跑酷区域:{info.Region.Name}");
                        player.SendInfoMessage($"跑酷奖励:{info.Award}金币");
                        player.SendInfoMessage($"跑酷奖励冷却:{info.AwardCD}小时");
                        player.SendInfoMessage($"跑酷排行榜坐标:{info.SignPos.X},{info.SignPos.Y}");
                    }
                    else
                    {
                        player.SendErrorMessage($"跑酷:{info.Name}不存在!");
                    }
                    break;
                case "sign":
                    //添加排行标牌
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("用法: /parkour sign [跑酷点名称]");
                        return;
                    }
                    //检测跑酷点是否存在
                    var signInfo = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (signInfo == null)
                    {
                        player.SendErrorMessage($"跑酷:{args.Parameters[1]}不存在");
                        return;
                    }
                    args.Player.SendSuccessMessage("编辑一个标牌作为计分板");
                    args.Player.SetData("ParkourSign", args.Parameters[1]);
                    break;

                case "start":
                    //检测玩家是否当前有进行的跑酷
                    bool flag = parkourPlays.Exists(x => x.player.Name == args.Player.Name);
                    if (flag)
                    {
                        parkourPlays.RemoveAll(x => x.player.Name == args.Player.Name);

                    }
                    //检测args.Parameters
                    if (args.Parameters.Count < 2)
                    {
                        args.Player.SendErrorMessage("[i:1311]用法: /parkour start [跑酷点名称]");
                        return;
                    }
                    var startInfo = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (startInfo == null)
                    {
                        player.SendErrorMessage($"[i:1311]跑酷:{args.Parameters[1]}不存在");
                        return;
                    }
                    parkourPlays.Add(new ParkourPlay(player, startInfo));
                    if (flag)
                    {
                        player.SendSuccessMessage($"[i:3099][{startInfo.Name}]已重置倒计时!");
                    }
                    else
                    {
                        player.SendSuccessMessage($"[i:3099][{startInfo.Name}]你开始了跑酷,现在开始计时!");

                    }
                    //检测是否奖励CD
                    if (!startInfo.AwardCDRecords.ContainsKey(args.Player.Account.ID) || DateTime.Now - startInfo.AwardCDRecords[args.Player.Account.ID] >= TimeSpan.FromHours(startInfo.AwardCD))
                    {
                        player.SendInfoMessage($"   [i:19]跑酷奖励:[c/FFD700:{startInfo.Award}金币]");
                    }
                    else
                    {
                        args.Player.SendErrorMessage($"   [i:20]跑酷{startInfo.Name}还在奖励CD中\n" +
                                                        $"   下次可领取奖励时间{(startInfo.AwardCDRecords[args.Player.Account.ID] + TimeSpan.FromHours(startInfo.AwardCD)).ToString()}");
                    }
                    args.Player.SendWarningMessage("输入'/退出跑酷'结束游戏");
                    break;
                case "end":
                    var groupMessage = "";
                    var endPlay = parkourPlays.Find(x => x.player.Name == args.Player.Name);
                    if (endPlay == null)
                    {
                        player.SendErrorMessage($"[i:1311]你没有进行的跑酷!");
                        return;
                    }
                    endPlay.End();
                    parkourPlays.RemoveAll(x => x.player.Name == args.Player.Name);
                    player.SendSuccessMessage($"[i:19][{endPlay.parkour.Name}]你完成了跑酷!!!\n" +
                        $"   [i:3099][c/40E0D0:当前用时:][c/1E90FF:{endPlay.GetFinalTime}][c/40E0D0:秒]\n" +
                        $"   [i:321][c/2F4F4F:死亡次数:][c/778899:{endPlay.DeathTimes}][c/2F4F4F:次]");

                    groupMessage += $"✅[{endPlay.parkour.Name}]玩家'{args.Player.Name}'完成了跑酷!!!\n" +
                        $"   ⏱️当前用时:{endPlay.GetFinalTime}秒\n" +
                        $"   👻死亡次数{endPlay.DeathTimes}次\n";

                    //检测是否为全服的新记录


                    if (endPlay.parkour.Records.Count == 0)
                    {
                        player.SendSuccessMessage($"[i:3867]跑酷点全服新记录!!!\n" +
                                                       $"   [i:4600]无记录 => [i:4601]{endPlay.GetFinalTime}秒");
                        groupMessage += $"📗跑酷点全服新记录!!!\n" +
                                                       $"   🥈无记录 => 🥇{endPlay.GetFinalTime}秒\n";
                        //额外奖励
                        args.Player.RewardPlayer(endPlay.parkour.Award * 2);
                        args.Player.SendWarningMessage($"   [i:2890]你收到了{endPlay.parkour.Award * 2}的跑酷新记录奖励!");

                    }

                    else if (endPlay.parkour.FastestRecord.Value > endPlay.totalTime)
                    {
                        player.SendSuccessMessage($"[i:3867]跑酷点全服新记录!!!\n" +
                           $"   [i:4600]{Math.Round(endPlay.parkour.FastestRecord.Value.TotalSeconds, 2)}秒 => [i:4601]{endPlay.GetFinalTime}秒");
                        //额外奖励
                        groupMessage += $"📗跑酷点全服新记录!!!\n" +
                               $"   🥈{Math.Round(endPlay.parkour.FastestRecord.Value.TotalSeconds, 2)} => 🥇{endPlay.GetFinalTime}秒\n";
                        args.Player.RewardPlayer(endPlay.parkour.Award * 2);
                        args.Player.SendWarningMessage($"   [i:2890]你收到了{endPlay.parkour.Award * 3}的跑酷新记录奖励!");

                    }


                    if (endPlay.parkour.Records.ContainsKey(args.Player.Account.ID))
                    {
                        if (endPlay.parkour.Records[args.Player.Account.ID] > endPlay.totalTime)
                        {
                            player.SendSuccessMessage($"[i:3867]跑酷点个人新记录!!!\n" +
                                $"   [i:4600]{endPlay.parkour.GetRecord(args.Player.Account.ID)}秒 => [i:4601]{endPlay.GetFinalTime}秒");
                            groupMessage += $"📘跑酷点个人新记录!!!\n" +
                               $"   🥈{endPlay.parkour.GetRecord(args.Player.Account.ID)}秒 => 🥇{endPlay.GetFinalTime}秒";
                            endPlay.parkour.Records[args.Player.Account.ID] = endPlay.totalTime;
                        }
                    }
                    else
                    {
                        endPlay.parkour.Records.Add(args.Player.Account.ID, endPlay.totalTime);

                    }
                    XSB.Utils.SendGroupMsg(groupMessage);
                    //奖励
                    if (!endPlay.parkour.AwardCDRecords.ContainsKey(args.Player.Account.ID) || DateTime.Now - endPlay.parkour.AwardCDRecords[args.Player.Account.ID] >= TimeSpan.FromHours(endPlay.parkour.AwardCD))
                    {
                        args.Player.RewardPlayer(endPlay.parkour.Award);
                        args.Player.SendWarningMessage($"[i:19]你收到了{endPlay.parkour.Award}金币的跑酷奖励");
                        if (endPlay.parkour.AwardCDRecords.ContainsKey(args.Player.Account.ID))
                        {
                            endPlay.parkour.AwardCDRecords[args.Player.Account.ID] = DateTime.Now;

                        }
                        else
                        {
                            endPlay.parkour.AwardCDRecords.Add(args.Player.Account.ID, DateTime.Now);
                        }
                        args.Player.SendWarningMessage($"   [i:3120]跑酷{endPlay.parkour.Name}进入奖励CD\n" +
                            $"   下次可领取奖励时间{(endPlay.parkour.AwardCDRecords[args.Player.Account.ID] + TimeSpan.FromHours(endPlay.parkour.AwardCD)).ToString()}");

                    }
                    else
                    {
                        args.Player.SendWarningMessage($"   [i:3120]跑酷{endPlay.parkour.Name}还在奖励CD中\n" +
                                                       $"   下次可领取奖励时间{(endPlay.parkour.AwardCDRecords[args.Player.Account.ID] + TimeSpan.FromHours(endPlay.parkour.AwardCD)).ToString()}");
                    }
                    break;
                case "setspawn":
                    var setSpawnParkour = parkourPlays.GetParkourByName(args.Player.Name);
                    if (setSpawnParkour == null)
                    {

                        player.SendErrorMessage($"[i:1311]你没有进行跑酷!");
                        return;
                    }
                    if (!setSpawnParkour.CanSetSpawn)
                    {
                        player.SendErrorMessage($"[i:224]复活5秒后才能设置记录点!");
                        return;
                    }
                    setSpawnParkour.SpawnPoint = args.Player.TPlayer.position;
                    player.SendWarningMessage($"[i:224]已设置记录点!");
                    break;
                case "delrecode":
                    //删除玩家的跑酷记录
                    if (args.Parameters.Count < 3)
                    {
                        player.SendErrorMessage($"[i:1311]用法:/parkour delrecode [c/FFD700:跑酷名字] [c/FFD700:玩家名]");
                        return;
                    }
                    var delRecodeParkour = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (delRecodeParkour == null)
                    {
                        player.SendErrorMessage($"[i:1311]跑酷{args.Parameters[1]}不存在!");
                        return;
                    }
                    var targetPlayer = TShock.UserAccounts.GetUserAccountByName(args.Parameters[2]);
                    if (targetPlayer == null)
                    {
                        player.SendErrorMessage($"[i:1311]玩家{args.Parameters[2]}不存在!");
                        return;
                    }
                    if (delRecodeParkour.Records.ContainsKey(targetPlayer.ID))
                    {
                        delRecodeParkour.Records.Remove(targetPlayer.ID);
                        player.SendSuccessMessage($"[i:1311]已删除玩家{targetPlayer.Name}的跑酷记录");
                    }
                    else
                    {
                        player.SendErrorMessage($"[i:1311]玩家{targetPlayer.Name}没有跑酷记录");
                    }
                    break;
                case "clearcd":
                    //删除玩家的奖励冷却CD，如果玩家名字是all或者*则全部清空
                    if (args.Parameters.Count < 3)
                    {
                        player.SendErrorMessage($"[i:1311]用法:/parkour clearcd [c/FFD700:跑酷点名称] [c/FFD700:玩家名]");
                        return;
                    }
                    var clearCDParkour = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (clearCDParkour == null)
                    {
                        player.SendErrorMessage($"[i:1311]跑酷{args.Parameters[1]}不存在!");
                        return;
                    }
                    //如果玩家名字是all或者*则全部清空
                    if (args.Parameters[2] == "all" || args.Parameters[2] == "*")
                    {
                        clearCDParkour.AwardCDRecords.Clear();
                        player.SendSuccessMessage($"[i:1311]已清除跑酷{clearCDParkour.Name}的所有玩家的跑酷奖励CD");
                        return;
                    }
                    var targetPlayer2 = TShock.UserAccounts.GetUserAccountByName(args.Parameters[2]);
                    if (targetPlayer2 == null)
                    {
                        player.SendErrorMessage($"[i:1311]玩家{args.Parameters[2]}不存在!");
                        return;
                    }
                    if (clearCDParkour.AwardCDRecords.ContainsKey(targetPlayer2.ID))
                    {
                        clearCDParkour.AwardCDRecords.Remove(targetPlayer2.ID);
                        player.SendSuccessMessage($"[i:1311]已清除玩家{targetPlayer2.Name}的跑酷奖励CD");
                    }
                    else
                    {
                        player.SendErrorMessage($"[i:1311]玩家{targetPlayer2.Name}没有跑酷奖励CD");
                    }
                    break;
                case "rank":
                    //以如下格式做一个排行榜，要可以用PaginationTools.SendPage翻页
                    //[第{n}名] 用户名字

                    //用法:/parkour rank [c/FFD700:跑酷点名称]
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage($"[i:1311]用法:/parkour rank [c/FFD700:跑酷点名称]");
                        return;
                    }
                    var rankParkour = parkourInfos.Find(x => x.Name == args.Parameters[1]);
                    if (rankParkour == null)
                    {
                        player.SendErrorMessage($"[i:1311]跑酷{args.Parameters[1]}不存在!");
                        return;
                    }
                    //先排序
                    var sortedRecords = rankParkour.Records.OrderBy(x => x.Value);
                    //显示前10名
                    var rank = 1;
                    List<string> rankList = new List<string>();
                    foreach (var record in sortedRecords)
                    {
                        if (rank > 10)
                        {
                            break;
                        }
                        var targetPlayer3 = TShock.UserAccounts.GetUserAccountByID(record.Key);
                        if (targetPlayer3 == null)
                        {
                            continue;
                        }
                        rankList.Add($"[第{rank}名] {targetPlayer3.Name}\n" +
                            $"#跑酷用时: {Math.Round(record.Value.TotalSeconds, 2)}秒");
                        rank++;
                    }
                    if (!PaginationTools.TryParsePageNumber(args.Parameters, 2, args.Player, out int pageNumber))
                        return;
                    PaginationTools.SendPage(
                    args.Player, pageNumber, rankList,
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "跑酷排行 ({0}/{1})：",
                        FooterFormat = "输入\"{0}跑酷排行 {1} [页码]\"查看更多".SFormat(Commands.Specifier, args.Parameters[1])
                    }
                );
                    break;
                default:
                    //显示命令列表
                    player.SendInfoMessage($"[i:1311]跑酷命令列表:\n" +
                                            $"/parkour add [c/FFD700:跑酷名字] - 创建跑酷\n" +
                                            $"/parkour del [c/FFD700:跑酷名字] - 删除跑酷\n" +
                                            $"/parkour start [c/FFD700:跑酷名字] - 开始跑酷\n" +
                                            $"/parkour end - 结束跑酷\n" +
                                            $"/parkour list - 查看跑酷列表\n" +
                                            $"/parkour info [c/FFD700:跑酷名字] - 查看跑酷信息\n" +
                                            $"/parkour setspawn [c/FFD700:跑酷名字] - 设置跑酷出生点\n" +
                                            $"/parkour delrecode [c/FFD700:跑酷名字] [c/FFD700:玩家名] - 删除玩家的跑酷记录\n" +
                                            $"/parkour clearcd [c/FFD700:跑酷点名称] [c/FFD700:玩家名] - 清除玩家的跑酷奖励CD\n" +
                                            $"/parkour rank [c/FFD700:跑酷名字] [c/FFD700:页数] - 查看跑酷排行榜");


                    break;
            }
        }



        private void OnSave(WorldSaveEventArgs args)
        {
            foreach (var i in parkourInfos)
            {
                DB.InsertParkour(i);
            }
        }




        private void RegionHooks_RegionLeft(RegionHooks.RegionLeftEventArgs args)
        {

            //检测若离开区域5秒结束游戏
            var play = parkourPlays.Find(x => x.player.Name == args.Player.Name);
            if (play != null)
            {
                Task.Run(delegate
                {
                    //for循环5次
                    for (int i = 0; i < 10; i++)
                    {
                        Thread.Sleep(500);
                        if (args.Player == null)
                        {
                            return;
                        }
                        if (args.Player.CurrentRegion == play.parkour.Region)
                        {
                            return;
                        }
                    }
                    args.Player.SendErrorMessage("[i:4085]离开跑酷区域5秒,跑酷结束!");
                    parkourPlays.RemoveAll(x => x.player.Name == args.Player.Name);
                });
            }
            //args.Player.SendSuccessMessage("[i:4085]你离开了跑酷区域,你的背包已切换!");
            //SSCSaver.RestoryBackBag(args.Player);

        }

        public ParkourInfo GetParkourByRegion(Region region)
        {
            //for (int i = 0; i < parkourInfos.Count; i++)
            //{
            //    Console.WriteLine(parkourInfos[i].Region.Name + " " + region.Name);
            //    if (parkourInfos[i].Region.Name == region.Name)
            //    {
            //        return parkourInfos[i];
            //    }
            //}改成foreach
            foreach (var i in parkourInfos)
            {
                if (i.Region == region)
                {
                    return i;
                }
            }
            return null;
        }
        private void RegionHooks_RegionEntered(RegionHooks.RegionEnteredEventArgs args)
        {
            if (args.Player.HasPermission("parkour.admin"))
            {
                return;
            }
            ParkourInfo info = GetParkourByRegion(args.Region);
            if (info != null)
            {
                //args.Player.SendSuccessMessage("[i:4084]你进入了跑酷区域,你的背包已切换!");
                SSCSaver.RestoryBag(args.Player, info.BagID, false);
            }

        }


    }
}
