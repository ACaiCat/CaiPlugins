﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Streams;
using System.Linq;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using PlaceholderAPI;
using static SwitchCommands.PluginCommands;
using static System.Net.Mime.MediaTypeNames;

namespace SwitchCommands
{
    [ApiVersion(2, 1)]
    public class SwitchCommands : TerrariaPlugin
    {

        public static Database database;

        public override string Name => "SwitchCommands";
        public override string Author => "Johuan&奇威复反汉化&Cai";
        public override string Description => "触发开关可以执行指令";
        public override Version Version => new Version(1, 2, 0, 0);

        public SwitchCommands(Main game) : base(game) { }

        public override void Initialize()
        {

            database = DB.LoadAll();
            TableManager.CreateTables();
            PluginCommands.RegisterCommands();
            ServerApi.Hooks.NetGetData.Register(this, GetData);

        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.NetGetData.Deregister(this, GetData);

            }
            base.Dispose(disposing);
        }

        private void GetData(GetDataEventArgs args)
        {
            using (MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length))
            {
                var player = TShock.Players[args.Msg.whoAmI];
                switch (args.MsgID)
                {
                    case PacketTypes.HitSwitch:
                        SwitchPos pos = new SwitchPos(data.ReadInt16(), data.ReadInt16());
                        var tile = Main.tile[pos.X, pos.Y];

                        if (tile.type == TileID.Lever)
                        {
                            if (tile.frameX % 36 == 0)
                                pos.X++;

                            if (tile.frameY == 0)
                                pos.Y++;
                        }

                        var playerState = player.GetData<PlayerState>("PlayerState");

                        if (playerState == PlayerState.SelectingSwitch)
                        {
                            player.SetData("SwitchPos", pos);
                            player.SendSuccessMessage("成功绑定位于X：{0}、Y：{1} 的开关".SFormat(pos.X, pos.Y));
                            player.SendSuccessMessage("输入/开关 ，可查看子命令列表".SFormat(pos.X, pos.Y));
                            player.SetData("PlayerState", PlayerState.AddingCommands);

                            if (database.switchCommandList.ContainsKey(pos.ToString()))
                            {
                                player.SetData("CommandInfo", database.switchCommandList[pos.ToString()]);
                            }

                            return;
                        }

                        if (playerState == PlayerState.None)
                        {
                            Console.WriteLine(pos.ToString());
                            if (database.switchCommandList.ContainsKey(pos.ToString()))
                            {
                                double seconds = 999999;


                                var cooldown = player.GetData<Dictionary<string, DateTime>>("冷却");

                                if (cooldown != null && cooldown.ContainsKey(pos.ToString()))
                                {
                                    seconds = (DateTime.Now - player.GetData<Dictionary<string, DateTime>>("冷却")[pos.ToString()]).TotalMilliseconds / 1000;
                                }

                                if (seconds < database.switchCommandList[pos.ToString()].cooldown)
                                {
                                    player.SendErrorMessage("开关冷却中, 请等待{0}秒!".SFormat(database.switchCommandList[pos.ToString()].cooldown - seconds));
                                    //冷却提示有点刷屏
                                    return;
                                }

                                
                                Group currGroup = null;

                                bool ignorePerms = database.switchCommandList[pos.ToString()].ignorePerms;

                                //定义一个事件当HitSwitch时触发
                                HitSwitchEvent.OnHitSwitch(pos.X, pos.Y, player);

                                foreach (string cmd in database.switchCommandList[pos.ToString()].commandList)
                                {
                                    if (ignorePerms)
                                    {
                                        currGroup = player.Group;
                                        player.Group = new SuperAdminGroup();
                                    }

                                    string Place = PlaceholderAPI.PlaceholderAPI.Instance.placeholderManager.GetText(cmd.ReplaceTags(player), player);
                                    Commands.HandleCommand(player, Place);
                                    if (ignorePerms)
                                    {
                                        player.Group = currGroup;
                                    }
                                }

                                if (cooldown == null)
                                {
                                    cooldown = new Dictionary<string, DateTime>() { { pos.ToString(), DateTime.Now } };
                                }
                                else
                                {
                                    cooldown[pos.ToString()] = DateTime.Now;
                                }

                                player.SetData("冷却", cooldown);
                            }
                        }

                        break;
                }

            }
        }
    }

    public static class StringManipulator
    {

        public static string ReplaceTags(this string s, TSPlayer player)
        {
            List<string> response = s.Split(' ').ToList();

            for (int x = response.Count - 1; x >= 0; x--)
                if (response[x] == "$name")
                    response[x] = "\"" + player.Name + "\"";
            return string.Join(" ", response);

        }

    }
}
