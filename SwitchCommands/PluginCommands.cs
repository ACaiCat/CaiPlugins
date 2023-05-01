﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlaceholderAPI;
using TShockAPI;

namespace SwitchCommands
{
    public class PluginCommands
    {
        public static string switchParameters = "/开关 <添加/列表/删除/冷却/权限忽略/取消/重绑/完成>";

        public static void RegisterCommands()
        {
            Commands.ChatCommands.Add(new Command("开关", SwitchCmd, "开关", "kg", "switch"));
            Commands.ChatCommands.Add(new Command("开关", SwitchReload, "重载开关", "reload"));
        }

        private static void SwitchReload(CommandArgs args)
        {
            SwitchCommands.database = DB.LoadAll();
            Console.WriteLine(string.Join("\n",SwitchCommands.database.switchCommandList.Keys));
            args.Player.SendSuccessMessage("[开关插件]开关重载成功！！！");
        }

        private static void SwitchCmd(CommandArgs args)
        {
            var player = args.Player;

            switch (player.GetData<PlayerState>("PlayerState"))
            {
                case PlayerState.None:
                    player.SendSuccessMessage("激活一个开关以将其绑定,之后可输入/开关 ，查看子命令");
                    player.SetData("PlayerState", PlayerState.SelectingSwitch);
                    return;

                case PlayerState.AddingCommands:
                    if (args.Parameters.Count == 0)
                    {
                        player.SendErrorMessage("正确指令：");
                        player.SendErrorMessage(switchParameters);
                        return;
                    }

                    if (player.GetData<CommandInfo>("CommandInfo") == null)
                        player.SetData("CommandInfo", new CommandInfo());

                    var cmdInfo = player.GetData<CommandInfo>("CommandInfo");

                    switch (args.Parameters[0].ToLower())
                    {
                        case "add":
                        case "添加":
                        case "tj":
                            var command = "/" + string.Join(" ", args.Parameters.Skip(1));
                            cmdInfo.commandList.Add(command);
                            player.SendSuccessMessage("成功添加: {0}".SFormat(command));
                            break;

                        case "list":
                        case "列表":
                        case "lb":
                            player.SendMessage("当前开关绑定的指令:", Color.Green);
                            for (int x = 0; x < cmdInfo.commandList.Count; x++)
                            {
                                player.SendMessage("({0}) ".SFormat(x) + cmdInfo.commandList[x], Color.Yellow);
                            }
                            break;

                        case "del":
                        case "删除":
                        case "sc":
                            int commandIndex = 0;

                            if (args.Parameters.Count < 2 || !int.TryParse(args.Parameters[1], out commandIndex))
                            {
                                player.SendErrorMessage("语法错误：/开关 del <指令>");
                                return;
                            }

                            var cmdDeleted = cmdInfo.commandList[commandIndex];
                            cmdInfo.commandList.RemoveAt(commandIndex);
                            player.SendSuccessMessage("成功删除了第{1}条指令：{0}。".SFormat(cmdDeleted, commandIndex));

                            break;

                        case "冷却":
                        case "cooldown":
                        case "lq":
                            float 冷却 = 0;

                            if (args.Parameters.Count < 2 || !float.TryParse(args.Parameters[1], out 冷却))
                            {
                                player.SendErrorMessage("语法错误：/开关 冷却 <秒>");
                                return;
                            }

                            cmdInfo.cooldown = 冷却;

                            player.SendSuccessMessage("冷却时间已设置为 {0} 秒".SFormat(冷却));
                            break;

                        case "权限忽略":
                        case "ignoreperms":
                        case "qxhl":
                            bool 权限忽略 = false;

                            if (args.Parameters.Count < 2 || !bool.TryParse(args.Parameters[1], out 权限忽略))
                            {
                                player.SendErrorMessage("语法错误：/开关 权限忽略 <true/false>");
                                return;
                            }

                            cmdInfo.ignorePerms = 权限忽略;

                            player.SendSuccessMessage("是否忽略玩家权限设置为: {0}.".SFormat(权限忽略));
                            break;

                        case "取消":
                        case "cancel":
                        case "qx":
                            player.SetData("PlayerState", PlayerState.None);
                            player.SetData("CommandInfo", new CommandInfo());
                            player.SendSuccessMessage("已取消添加要添加的命令");
                            return;

                        case "重绑":
                        case "rebind":
                        case "zb":
                            player.SendSuccessMessage("重新激活开关后可以重新绑定");
                            player.SetData("PlayerState", PlayerState.SelectingSwitch);
                            return;

                        case "完成":
                        case "done":
                        case "wc":
                            var switchPos = player.GetData<SwitchPos>("SwitchPos");

                            player.SendSuccessMessage("设置成功的开关位于 X： {0}， Y： {1}".SFormat(switchPos.X, switchPos.Y));
                            foreach (string cmd in cmdInfo.commandList)
                            {
                                player.SendMessage(cmd, Color.Yellow);
                            }
                            cmdInfo.point = switchPos;
                            SwitchCommands.database.switchCommandList[player.GetData<SwitchPos>("SwitchPos").ToString()] = cmdInfo;
                            player.SetData("PlayerState", PlayerState.None);
                            player.SetData("SwitchPos", new Vector2());
                            player.SetData("CommandInfo", new CommandInfo());
                            DB.SaveAll(SwitchCommands.database);
                            return;

                        default:
                            player.SendErrorMessage("语法无效. " + switchParameters);
                            return;
                    }
                     
                    player.SetData("CommandInfo", cmdInfo);

                    return;
            }
        }

        public enum PlayerState
        {
            None,
            AddingCommands,
            SelectingSwitch
        }
    }
}
