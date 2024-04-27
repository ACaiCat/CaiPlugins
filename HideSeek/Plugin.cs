using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using HideSeek;
using StatusTxtMgr;
using Steamworks;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using Version = System.Version;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        //定义插件的作者名称
        public override string Author => "Cai";

        //插件的一句话描述
        public override string Description => "躲猫猫";

        //插件的名称
        public override string Name => "躲猫猫";

        //插件的版本
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        //插件的构造器
        public Plugin(Main game) : base(game)
        {
        }
        Game Game = new Game();
        //插件加载时执行的代码
        int GameStart = 60;
        public override void Initialize()
        {
            Config.Read();
            GetDataHandlers.PlayerSpawn.Register(OnSpawn);
            //On.Terraria.NetMessage.SendData += NetMessage_SendData; ;
            ServerApi.Hooks.GamePostUpdate.Register(this, GameUpdata);
            ServerApi.Hooks.ServerLeave.Register(this, GameLeave);
            //恋恋给出的模板代码中展示了如何为TShock添加一个指令
            Commands.ChatCommands.Add(new Command(
                cmd: this.Cmd,
                "退出躲猫猫"));
            Commands.ChatCommands.Add(new Command(
                cmd: this.JoinCmd,
                "加入躲猫猫"));
            Commands.ChatCommands.Add(new Command(
                permissions: "躲猫猫.管理",
                cmd: this.SetCmd,
                "设置躲猫猫", "setcat"));
            StatusTxtMgr.StatusTxtMgr.Hooks.StatusTextUpdate.Register(delegate (StatusTextUpdateEventArgs args)
            {
                var tsplayer = args.tsplayer;
                var statusTextBuilder = args.statusTextBuilder;
                statusTextBuilder.AppendLine();
                if (Game.Players.Select(i => i.TSPlayer).Contains(tsplayer))
                {
                    if (Game.isGameing)
                    {
                        string role = "";
                        var plr = Game.Players.Find(i => i.TSPlayer == tsplayer);
                        switch (plr.Role)
                        {
                            case Role.Cat:
                                role = "[c/c35a15:猫猫]";
                                break;
                            case Role.Mouse:
                                role = "[c/5a5ae2:鼠鼠]";
                                break;
                            default
:
                                role = "旁观者";
                                break;


                        }
                        statusTextBuilder.AppendLine("[c/d8228f:躲][c/d737aa:猫][c/d14ac6:猫][c/c65de0:模][c/b570fa:式]([c/00d517:游戏中])");
                        statusTextBuilder.AppendLine($"[i:603]当前身份:{role}");
                        statusTextBuilder.AppendLine($"[i:3457]当前状态:{(plr.isDead ? "[c/556b5d:已死亡]" : "[c/00f253:存活]")}");
                        statusTextBuilder.AppendLine($"[i:17]剩余时间:[c/e54037:{(Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds)}]秒");
                        statusTextBuilder.AppendLine($"[i:1810]剩余猫猫:[c/c35a15:{Game.CatPlayers.Count(i => !i.isDead)}]");
                        statusTextBuilder.AppendLine($"[i:2163]剩余鼠鼠:[c/5a5ae2:{Game.MousePlayers.Count(i => !i.isDead)}]");
                    }
                    else
                    {
                        var role = "[c/556b5d:未选择]";
                        var roleTag = "[g:0]";
                        if (tsplayer.SelectedItem.netID == 1810)
                        {
                            role = "[c/c35a15:猫猫]";
                            roleTag = "[i:1810]";
                        }
                        if (tsplayer.SelectedItem.netID == 2163)
                        {
                            role = "[c/5a5ae2:鼠鼠]";
                            roleTag = "[i:2163]";
                        }

                        statusTextBuilder.AppendLine($"[c/d8228f:躲][c/d737aa:猫][c/d14ac6:猫][c/c65de0:模][c/b570fa:式]([c/dd4e1c:未开始])");
                        statusTextBuilder.AppendLine($"{roleTag}当前选择身份:{role}");
                        statusTextBuilder.AppendLine($"[i:425]当前玩家数:{Game.Players.Count}");
                        if (Game.Players.Count == 1)
                        {
                            statusTextBuilder.AppendLine("[i:17]开始倒计时:[c/e54037:人数不足]");
                        }
                        else
                        {
                            statusTextBuilder.AppendLine($"[i:17]开始倒计时:[c/e54037:{GameStart}]秒");
                        }

                    }

                }


            });
        }

        private void NetMessage_SendData(On.Terraria.NetMessage.orig_SendData orig, int msgType, int remoteClient, int ignoreClient, Terraria.Localization.NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            if (msgType == (int)PacketTypes.PlayerUpdate)
            {
                try
                {
                    if (Game.Players.Where(i => i.TSPlayer == TShock.Players[remoteClient]).First().isDead)
                    {
                        return;
                    }
                }
                catch { }

            }
            orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
        }

        private void GameLeave(LeaveEventArgs args)
        {
            if (Game.Players.Select(i => i.TSPlayer).Contains(TShock.Players[args.Who]))
            {
                var plr = Game.Players.Find(i => i.TSPlayer == TShock.Players[args.Who]);
                if (plr.Role == Role.Cat)
                {
                    Game.Broadcast($"[i:321]猫猫[c/c35a15:{plr.AccountName}]退出了游戏,剩余[c/c35a15:{Game.CatPlayers.Count(i => !i.isDead)}]只猫猫！");
                }
                if (plr.Role == Role.Mouse)
                {
                    Game.Broadcast($"[i:321]鼠鼠[c/5a5ae2:{plr.AccountName}]退出了游戏,剩余[c/5a5ae2:{Game.MousePlayers.Count(i => !i.isDead)}]只鼠鼠!");
                }
                Game.Players.RemoveAll(i => i.TSPlayer == TShock.Players[args.Who]);
            }
        }

        private void SetCmd(CommandArgs args)
        {
            var player = args.Player;
            if (args.Parameters.Count == 0)
            {
                player.SendInfoMessage($"[i:1991]躲猫猫设置列表:\n" +
                        $"/setcat winmoney <金币数> - 设置胜利奖励\n" +
                        $"/setcat losemoney <金币数> - 设置失败奖励\n" +
                        $"/setcat pressc <SSCID> - 设置准备背包ID\n" +
                        $"/setcat catssc <SSCID> - 设置猫背包ID\n" +
                        $"/setcat mousessc <SSCID> - 设置鼠背包ID\n" +
                        $"/setcat gameroom - 设置游戏准备房间\n" +
                        $"/setcat catstart - 设置猫初始位置\n" +
                        $"/setcat mousestart - 设置鼠初始位置\n" +
                        $"/setcat additem <物品ID> - 添加物品生成点\n" +
                        $"/setcat delitem - 清空物品生成点\n" +
                        $"/setcat region - <区域名> - 设置躲猫猫区域");
                return;
            }
            switch (args.Parameters[0].ToLower())
            {
                case "winmoney":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat winmoney <金币数>");
                        return;
                    }
                    Config.config.WinMoney = int.Parse(args.Parameters[1]);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "region":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat region <区域名>");
                        return;
                    }
                    Config.config.RegionName = args.Parameters[1];
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "losemoney":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat losemoney <金币数>");
                        return;
                    }
                    Config.config.LoseMoney = int.Parse(args.Parameters[1]);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "pressc":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat pressc <SSCID>");
                        return;
                    }
                    Config.config.JoinSSC = int.Parse(args.Parameters[1]);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "catssc":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat catssc <SSCID>");
                        return;
                    }
                    Config.config.CatSSC = int.Parse(args.Parameters[1]);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "mousessc":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat mousessc <SSCID>");
                        return;
                    }
                    Config.config.MouseSSC = int.Parse(args.Parameters[1]);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "gameroom":
                    Config.config.GameRoom = new System.Drawing.Point((int)args.Player.X, (int)args.Player.Y);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "catstart":
                    Config.config.CatStart = new System.Drawing.Point((int)args.Player.X, (int)args.Player.Y);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "mousestart":
                    Config.config.MouseStart = new System.Drawing.Point((int)args.Player.X, (int)args.Player.Y);
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "additem":
                    if (args.Parameters.Count < 2)
                    {
                        player.SendErrorMessage("格式错误，正确格式: /setcat additem <物品ID> ");
                        return;
                    }
                    Config.config.ItemSpawn.Add(new System.Drawing.Point((int)args.Player.X, (int)args.Player.Y), int.Parse(args.Parameters[1]));
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                case "delitem":
                    Config.config.ItemSpawn.Clear();
                    Config.config.Write();
                    player.SendSuccessMessage("设置成功");
                    break;
                default:
                    player.SendInfoMessage($"[i:1991]躲猫猫设置列表:\n" +
                       $"/setcat winmoney <金币数> - 设置胜利奖励\n" +
                       $"/setcat losemoney <金币数> - 设置失败奖励\n" +
                       $"/setcat pressc <SSCID> - 设置准备背包ID\n" +
                       $"/setcat catssc <SSCID> - 设置猫背包ID\n" +
                       $"/setcat mousessc <SSCID> - 设置鼠背包ID\n" +
                       $"/setcat gameroom - 设置游戏准备房间\n" +
                       $"/setcat catstart - 设置猫初始位置\n" +
                       $"/setcat mousestart - 设置鼠初始位置\n" +
                       $"/setcat additem <物品ID> - 添加物品生成点\n" +
                       $"/setcat delitem - 清空物品生成点");
                    return;


            }
        }

        private void JoinCmd(CommandArgs args)
        {
            if (Game.isWaitEnd)
            {
                args.Player.SendErrorMessage("[i:1991]游戏正在等待结算！");
                return;
            }
            if (Game.Players.Select(i => i.TSPlayer).Contains(args.Player))
            {
                args.Player.SendErrorMessage("[i:1991]你已经在游戏中了");
                return;
            }

            Game.Join(args.Player);

        }

        int _tick = 0;
        int remind = -1;
        private void GameUpdata(EventArgs args)
        {
            if (_tick % 60 == 0)
            {
                if (Game.isGameing)
                {
                    if (Game.isGameMouseDead || Game.isGameTimeOut || Game.isGameCatDead)
                    {
                        if (!Game.isWaitEnd)
                            Game.GameEnd();
                        return;
                    }
                    if (remind != (Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds))
                    {
                        if ((Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds) % 15 == 0)
                        {
                            remind = ((Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds));
                            Game.Broadcast($"[i:1991]游戏还有[c/e54037:{(Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds)}]秒结束!");
                            Game.Broadcast($"*还有[c/5a5ae2:{Game.CatPlayers.Count(i => !i.isDead)}]只猫猫,[c/c35a15:{Game.MousePlayers.Count(i => !i.isDead)}]只鼠鼠!");
                        }
                        if ((Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds) == 60)
                        {
                            Game.Broadcast($"[i:3183]喵喵们急了，变成了猎喵!(移速增加)");
                        }
                        if ((Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds) == 30)
                        {
                            Game.Broadcast($"[i:3183]喵喵们更急了，变成了终极猎喵!(活得狩猎效果,速度大幅增加)");
                        }
                    }
                    if ((Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds) <= 60)
                    {
                        foreach (var i in Game.CatPlayers)
                        {
                            if (i.TSPlayer != null)
                            {
                                i.TSPlayer.SetBuff(3, 360, true);
                            }
                        }
                    }
                    if ((Config.config.GameLast - (int)(DateTime.Now - Game.GameStartTime).TotalSeconds) <= 30)
                    {
                        foreach (var i in Game.CatPlayers)
                        {
                            if (i.TSPlayer != null)
                            {
                                i.TSPlayer.SetBuff(17, 360, true);
                                i.TSPlayer.SetBuff(63, 360, true);

                            }
                        }
                        foreach (var i in Game.MousePlayers)
                        {
                            if (i.TSPlayer != null)
                            {
                                i.TSPlayer.SetBuff(11, 360, true);

                            }
                        }
                    }
                    foreach (var i in Game.Players.Where(i => i.isDead))
                    {
                        if (i.TSPlayer == null)
                        {
                            continue;
                        }
                        i.TSPlayer.SetBuff(10, 500, true);
                        i.TSPlayer.SetBuff(3, 500, true);

                    }



                }
                else
                {
                    
                    if (Game.Players.Count > 1)
                    {
                        if (GameStart % 10 == 0)
                        {
                            Game.Broadcast($"[i:1991]游戏将在[c/e54037:{GameStart}]秒后开始");
                        }
                        GameStart--;
                    }
                    else
                    {
                        GameStart = 60;
                    }
                    if (GameStart == 0)
                    {
                        Game.GameStart();
                        GameStart = 60;
                    }
                }
            }
            if (_tick == 300)
            {
                _tick = 0;
                Game.GameSpawnItem();
            }
            _tick++;


        }

        private void OnSpawn(object? sender, GetDataHandlers.SpawnEventArgs e)
        {
            if (Game.Players.Select(i => i.TSPlayer).Contains(e.Player))
            {
                if (Game.isGameing)
                {
                    Task.Run(() =>
                    {

                        if (e.Player != null)
                        {
                            var plr = Game.Players.Find(i => i.TSPlayer == e.Player);
                            plr.isDead = true;
                            if (plr.Role == Role.Cat)
                            {
                                Game.Broadcast($"[i:321]猫猫[c/5a5ae2:{plr.AccountName}]挂了,剩余[c/5a5ae2:{Game.CatPlayers.Count(i => !i.isDead)}]只猫猫！");
                            }
                            if (plr.Role == Role.Mouse)
                            {
                                Game.Broadcast($"[i:321]鼠鼠[c/5a5ae2:{plr.AccountName}]挂了,剩余[c/5a5ae2:{Game.MousePlayers.Count(i => !i.isDead)}]只鼠鼠!");
                            }
                            e.Player.SendWarningMessage("[i:321]你挂了,已为你开启旁观者模式！");
                            e.Player.SendWarningMessage("你可以使用'/退出躲猫猫'退出游戏(将会失去所有奖励)");
                            Thread.Sleep(1000);
                            if (e.Player == null)
                            {
                                return;
                            }

                            e.Player.Teleport(Config.config.MouseStart.X, Config.config.MouseStart.Y);
                            e.Player.TPlayer.ghost = true;

                            e.Player.SendData(PacketTypes.PlayerUpdate, null, e.Player.Index);
                            e.Player.SetPvP(false);

                        }

                    });

                }
            }
        }

        //插件卸载时执行的代码

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //移除所有由本插件添加的所有指令

                var asm = Assembly.GetExecutingAssembly();
                Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
            }
            base.Dispose(disposing);
        }

        //执行指令时对指令进行处理的方法
        private void Cmd(CommandArgs args)
        {
            if (Game.Players.Select(i => i.TSPlayer).Contains(args.Player))
            {
                var plr = Game.Players.Find(i => i.TSPlayer == args.Player);
                if (plr.Role == Role.Cat)
                {
                    Game.Broadcast($"[i:321]猫猫[c/5a5ae2:{plr.AccountName}]退出了游戏,剩余[c/5a5ae2:{Game.CatPlayers.Count(i => !i.isDead)}]只猫猫！");
                }
                if (plr.Role == Role.Mouse)
                {
                    Game.Broadcast($"[i:321]鼠鼠[c/5a5ae2:{plr.AccountName}]退出了游戏,剩余[c/5a5ae2:{Game.MousePlayers.Count(i => !i.isDead)}]只鼠鼠!");
                }
                Game.Players.RemoveAll(i => i.TSPlayer == args.Player);
                args.Player.SendSuccessMessage("[i:1991]已退出躲猫猫游戏");
                args.Player.TPlayer.ghost = false;
                args.Player.SendData(PacketTypes.PlayerUpdate, null, args.Player.Index);

            }
            else
            {
                args.Player.SendErrorMessage("[i:1991]你不在游戏中...");
            }
        }
    }
}