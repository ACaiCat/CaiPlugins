using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using System.Data;
using TShockAPI.Hooks;
using System.Runtime.CompilerServices;

namespace SSCManager
{
    [ApiVersion(2, 1)]
    public class SSCSaver : TerrariaPlugin
    {
        public SSCSaver(Terraria.Main game) : base(game) { }

        public override string Name => "SSCSaver";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => "Cai";

        public override string Description => "SSC Saver";

        public override void Initialize()
        {
            TableManager.CreateTables();
            Commands.ChatCommands.Add(new Command("ssc.admin", CommandHandler, "ssc")
            {
            });
            ServerApi.Hooks.ServerLeave.Register(this, Onleave,int.MaxValue);

        }

        private void Onleave(LeaveEventArgs args)
        {
           if (playerDatas[args.Who] != null)
            {
                RestoryBackBag(TShock.Players[args.Who]);
            }
        }

        //private void RegionHooks_RegionEntered(RegionHooks.RegionEnteredEventArgs args)
        //{
        //    args.Player.PlayerData = SSCDB.GetPlayerData(2);
        //    args.Player.PlayerData.RestoreCharacter(TShock.Players[args.Player.Index]);
        //}

        public static PlayerData[] playerDatas = new PlayerData[255];
        public static void RestoryBag(TSPlayer plr,int sscid,bool savessc = true)
        {
            if (savessc)
            {
                playerDatas[plr.Index] = plr.PlayerData;
            }
            plr.PlayerData = SSCDB.GetPlayerData(sscid);
            plr.PlayerData.RestoreCharacter(plr);
            plr.Heal(plr.PlayerData.maxHealth);
        }

        public static PlayerData GetDate(int sscid)
        {
            return SSCDB.GetPlayerData(sscid);
        }

        public static bool ExistBag(int id)
        {
            return SSCDB.GetPlayerData(id).exists;
        }
        public static void RestoryBackBag(TSPlayer plr)
        {
            
            plr.PlayerData = playerDatas[plr.Index];
            plr.PlayerData.RestoreCharacter(plr);
            playerDatas[plr.Index] = null;
        }

        private void CommandHandler(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendErrorMessage("无效的SSC管理器子命令!有效命令如下:\n" +
                        "ssc save [ID]--保存SSC\n" +
                        "ssc del [ID]--删除SSC\n" +
                        "ssc list --列出SSC\n" +
                        "ssc restore --复原SSC");
                return;
            }
            int sscid = -1;
            switch (args.Parameters[0].ToLower())
            {
                case "保存":
                case "save":
                    if (args.Parameters.Count != 2 || !int.TryParse(args.Parameters[1], out sscid))
                    {
                        args.Player.SendErrorMessage("用法错误!正确用法:ssc save [ID]");
                        return;
                    }

                    if (SSCDB.InsertPlayerData(args.Player, sscid))
                    {
                        args.Player.SendSuccessMessage("保存成功!");
                        return;
                    }
                    args.Player.SendErrorMessage("保存失败!");
                    break;
                case "删除":
                case "del":
                    if (args.Parameters.Count != 2 || !int.TryParse(args.Parameters[1], out sscid))
                    {
                        args.Player.SendErrorMessage("用法错误!正确用法:ssc del [ID]");
                        return;
                    }
                    if (!SSCDB.GetPlayerData(sscid).exists)
                    {
                        args.Player.SendErrorMessage("你输入的SSC背包ID不存在!");
                        return;
                    }
                    if (SSCDB.DeletePlayerData(sscid))
                    {
                        args.Player.SendSuccessMessage("删除成功!");
                        return;
                    }
                    args.Player.SendErrorMessage("删除失败!");
                    break;
                case "列出":
                case "list":
                    args.Player.SendSuccessMessage("有效的SSC背包列表:" + string.Join(',', SSCDB.GetAllSSCId()));
                    break;
                case "还原背包":
                case "restore":
                    if (args.Parameters.Count != 2 || !int.TryParse(args.Parameters[1], out sscid))
                    {
                        args.Player.SendErrorMessage("用法错误!正确用法:ssc restore [ID]");
                        return;
                    }
                    if (!SSCDB.GetPlayerData(sscid).exists)
                    {
                        args.Player.SendErrorMessage("你输入的SSC背包ID不存在!");
                        return;
                    }
                    args.Player.PlayerData = SSCDB.GetPlayerData(sscid);
                    args.Player.PlayerData.RestoreCharacter(args.Player);
                    args.Player.Heal(args.Player.PlayerData.maxHealth);
                    args.Player.SendSuccessMessage("背包已还原!");
                    break;
                default:
                    args.Player.SendErrorMessage("无效的SSC管理器子命令!有效命令如下:\n" +
                        "ssc save [ID]--保存SSC\n" +
                        "ssc del [ID]--删除SSC\n" +
                        "ssc list --列出SSC\n" +
                        "ssc restore --复原SSC");
                    break;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                //ServerApi.Hooks.GamePostUpdate.Deregister(this, OnPostUpdate);
            }
            base.Dispose(disposing);
        }
    }
}
