using System.Drawing;
using System.IO.Streams;
using System.Text;
using IL.Terraria.Chat.Commands;
using SSCManager;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using TShockAPI.Net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using TShockAPI.DB;
using TerrariaApi.Server;
using TShockAPI.Hooks;
using Terraria.GameContent.Events;
using Microsoft.Xna.Framework;
using TShockAPI.Localization;
using System.Text.RegularExpressions;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;

namespace LobbyManager
{
    [ApiVersion(2, 1)]
    public class LobbyManager : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "主城综合插件";

        public override string Name => "LobbyManager";

        public override Version Version => new Version(1, 0, 0, 0);
        public static PlayerData data { get; set; }
        public LobbyManager(Main game)
        : base(game)
        {
        }
        public static Config config { get; set; }
        public override void Initialize()
        {
            Config.GetConfig();
            //RegionHooks.RegionLeft += RegionHooks_RegionLeft;
            PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            GeneralHooks.ReloadEvent += OnReload;
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            Commands.ChatCommands.RemoveAll(i => i.Name == "spawnmob");
            Commands.ChatCommands.Add(new Command(Permissions.spawnmob, SpawnMob, "spawnmob", "sm")
            {
                AllowServer = false,
                //HelpText = GetString("Spawns a number of mobs around you.")
            });

            //当世界加载完成

        }

        private static void SpawnMob(CommandArgs args)
        {
            if (args.Parameters.Count < 1 || args.Parameters.Count > 2)
            {
                args.Player.SendErrorMessage("格式错误!正确格式: /spawnmob <mob种类> [数量].");
                return;
            }
            if (args.Parameters[0].Length == 0)
            {
                args.Player.SendErrorMessage("无效怪物类型");
                return;
            }

            int amount = 1;
            if (args.Parameters.Count == 2 && !int.TryParse(args.Parameters[1], out amount))
            {
                args.Player.SendErrorMessage("格式错误!正确格式: /spawnmob <mob种类> [数量].");
                return;
            }

            amount = Math.Min(amount, Main.maxNPCs);

            var npcs = TShock.Utils.GetNPCByIdOrName(args.Parameters[0]);
            if (npcs.Count == 0)
            {
                args.Player.SendErrorMessage("无效怪物类型");
            }
            else if (npcs.Count > 1)
            {
                args.Player.SendMultipleMatchError(npcs.Select(n => $"{n.FullName}({n.type})"));
            }
            else
            {
                var npc = npcs[0];
                if (npc.type >= 1 && npc.type < Terraria.ID.NPCID.Count && npc.type != 113)
                {
                    TSPlayer.Server.SpawnNPC(npc.netID, npc.FullName, amount, args.Player.TileX, args.Player.TileY, 50, 20);
                    //if (args.Silent)
                    //{
                    //    //args.Player.SendSuccessMessage(GetPluralString("Spawned {0} {1} time.", "Spawned {0} {1} times.", amount, npc.FullName, amount));
                    //}
                    //else
                    //{
                    //    TSPlayer.All.SendSuccessMessage(GetPluralString("{0} has spawned {1} {2} time.", "{0} has spawned {1} {2} times.", amount, args.Player.Name, npc.FullName, amount));
                    //}
                }
                else if (npc.type == 113)
                {
                    if (Main.wofNPCIndex != -1 || (args.Player.Y / 16f < (Main.maxTilesY - 205)))
                    {
                        args.Player.SendErrorMessage("无法把肉山生成在当前位置");
                        return;
                    }
                    NPC.SpawnWOF(new Vector2(args.Player.X, args.Player.Y));
                    //if (args.Silent)
                    //{
                    //    args.Player.SendSuccessMessage(GetString("Spawned a Wall of Flesh."));
                    //}
                    //else
                    //{
                    //    TSPlayer.All.SendSuccessMessage(GetString("{0} has spawned a Wall of Flesh.", args.Player.Name));
                }
                else
                {
                    args.Player.SendErrorMessage("无效怪物类型");
                }
            }
        }


        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.SyncLoadout)
            {
                var plr = TShock.Players[args.Msg.whoAmI];
                plr.TPlayer.CurrentLoadoutIndex = 0;
                plr.SendData(PacketTypes.SyncLoadout, "", plr.Index);
                args.Handled = true;
                plr.SendWarningMessage("[i:5325]本服务器不允许切换装备栏!");
            }

        }

        private void OnReload(ReloadEventArgs e)
        {
            Config.GetConfig();
            e.Player.SendSuccessMessage("[i:50][主城综合]配置文件已重载!");


        }




        private void OnJoin(JoinEventArgs args)
        {
            if (TShock.Players[args.Who].HasPermission("lobby.ignore"))
            {
                return;
            }
            TShock.Players[args.Who].IgnoreSSCPackets = true;
        }

        private void PlayerHooks_PlayerPostLogin(PlayerPostLoginEventArgs e)
        {
            if (e.Player.HasPermission("lobby.ignore"))
            {
                return;
            }
            e.Player.RestoryBag(config.SSCId);
        }

        private void RegionHooks_RegionLeft(RegionHooks.RegionLeftEventArgs args)
        {
            if (args.Player.HasPermission("lobby.ignore"))
            {
                return;
            }
            args.Player.RestoryBag(config.SSCId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //RegionHooks.RegionLeft -= RegionHooks_RegionLeft;
                PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }


    }
}
