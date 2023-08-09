using MySqlX.XDevAPI.Common;
using System.Collections.Generic;
using System.Text;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using StatusTxtMgr;
using System.Net.NetworkInformation;
using Ping;
using System.Runtime.CompilerServices;
using Terraria.Localization;
using StatusInfo;
using System.Timers;
using TShockAPI.Hooks;

namespace GameStatus
{
    [ApiVersion(2, 1)]
    public class GameStatus : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "生存用计分板";

        public override string Name => "生存用计分板";

        public override Version Version => new Version(1, 0, 0, 0);

        public GameStatus(Main game)
        : base(game)
        {
        }
        public Config config = new Config();
        public static int totalOnline = 0;
        System.Timers.Timer timer = new System.Timers.Timer(5000);

        public readonly List<string> tags = new()
        {
            {"{玩家名}"},{"{玩家组名}"},{"{玩家血量}"},{"{玩家血量最大值}"},{"{玩家魔力}"},{"{玩家魔力最大值}"},{"{玩家幸运值}"},{"{玩家X坐标}"},{"{玩家Y坐标}"},{"{玩家所处区域}"},
            {"{玩家手持物品.图标}"},{"{玩家手持物品.数量}"},{"{玩家手持物品.修饰语}"},{"{玩家手持物品.名字}"},{"{玩家死亡状态}"},{"{重生倒计时}" },{ "{智能死亡时间}"},{"{当前环境}"},
            {"{当前服务器在线人数}"},{"{所有服务器总在线人数}"},{"{玩家延迟}"},{"{全服平均延迟}"},{"{服务器帧率}"},{"{今日渔夫任务}"},{"{地图名称}"},{ "{当前时间}"}
        };
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("", Status, "status", "状态", "ping", "tps", "fps"));
            GeneralHooks.ReloadEvent += GeneralHooks_ReloadEvent;
            config = Config.Read();
            for (int i = 0; i < tags.Count; i++)
            {
                config.Content = config.Content.Replace(tags[i], "{" + i + "}");
            }
            //{玩家名}{玩家组名}{玩家血量}{玩家血量最大值}{玩家魔力}{玩家魔力最大值}{玩家幸运值}{玩家X坐标}{玩家Y坐标}{玩家所处区域}
            //{玩家手持物品.图标}{玩家手持物品.数量}{玩家手持物品.修饰语}{玩家手持物品.名字}{玩家死亡状态}{重生倒计时}{当前环境}
            //{当前服务器在线人数}{所有服务器总在线人数}{玩家延迟}{全服平均延迟}{服务器帧率}{今日渔夫任务}{地图名称}


            StatusTxtMgr.StatusTxtMgr.Hooks.StatusTextUpdate.Register(delegate (StatusTextUpdateEventArgs args)
            {
                var plr = args.tsplayer;
                var statusTextBuilder = args.statusTextBuilder!;
            statusTextBuilder.AppendLine(string.Format(config.Content, plr.Name, plr.Group.Name, plr.TPlayer.statLife, plr.TPlayer.statLifeMax2
                , plr.TPlayer.statMana, plr.TPlayer.statManaMax2, plr.GetLuck(), plr.TileX, plr.TileY, plr.CurrentRegion == null ? "空区域" : plr.CurrentRegion.Name
                , string.IsNullOrEmpty(TShock.Utils.ItemTag(plr.TPlayer.HeldItem))?"[i:1]": TShock.Utils.ItemTag(plr.TPlayer.HeldItem), plr.TPlayer.HeldItem.stack, Lang.prefix[plr.TPlayer.HeldItem.prefix].Value, string.IsNullOrEmpty(plr.TPlayer.HeldItem.Name)?"空格位":plr.TPlayer.HeldItem.Name
                ,plr.Dead?"已死亡":"存活",plr.RespawnTimer, plr.RespawnTimer==0? "":$"[i:321]复活倒计时:[c/CCCCCC:{plr.RespawnTimer}秒]",plr.GetEnvString(),TShock.Utils.GetActivePlayerCount(),totalOnline,Ping.Ping.GetPing(plr),Ping.Ping.GetAveragePing(),
                SBPlugin.ServerFPS.GetServerFPS(),plr.GetFisheMission(),Main.worldName,DateTime.Now.ToString("HH:mm")));
            }, 60uL);

            timer.Interval = config.RefreshSpeed;
            timer.Elapsed += GetAllServerOnline;
            timer.Enabled = true;
            timer.Start();

        }
        private void Status(CommandArgs args)
        {
            //分别发送全服玩家的延迟
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("当前服务器状态:");
            sb.AppendLine("[i:17]平均延迟:" + Ping.Ping.GetAveragePing());
            sb.AppendLine("[i:3099]服务器帧率(波动):" + SBPlugin.ServerFPS.GetServerFPS()); 
            //发送全服玩家的延迟
            sb.AppendLine("[i:267]全服玩家延迟:");
            foreach (var i in TShock.Players)
            {
                if (i != null)
                {
                    sb.AppendLine(i.Name + ":" + Ping.Ping.GetPing(i));
                }
            }
            args.Player.SendInfoMessage(sb.ToString());


        }
        private void GeneralHooks_ReloadEvent(ReloadEventArgs e)
        {
            config = Config.Read();
            timer.Interval = config.RefreshSpeed;
            for (int i = 0; i < tags.Count; i++)
            {
                config.Content = config.Content.Replace(tags[i], "{" + i + "}");
            }
            e.Player.SendSuccessMessage("[i:50][计分板]配置文件已重载!");
        }

        private void GetAllServerOnline(object? sender, ElapsedEventArgs e)
        {
            if (!config.EndableRestWho) { return; }
            int onlines = 0;
            
            foreach (var i in config.Rests)
            {
                onlines += StatusInfo.Utils.GetServerOnline(i.Key, i.Value);
            }
            totalOnline = onlines + TShock.Utils.GetActivePlayerCount();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                timer.Stop();
                timer.Dispose();
            }
            base.Dispose(disposing);
        }


    }
    public static class Tools
    {

        public static string GetFisheMission(this TSPlayer plr)
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (CheckNPCActive("369"))
            {

                int itemID = Main.anglerQuestItemNetIDs[Main.anglerQuest];
                bool finish = Main.anglerWhoFinishedToday.Exists((string x) => x == plr.Name);
                string questText3 = Language.GetTextValue("AnglerQuestText.Quest_" + ItemID.Search.GetName(itemID));
                string[] splits = questText3.Split("\n\n".ToCharArray());
                if (splits.Count() > 1)
                {
                    questText3 = splits[splits.Count() - 1];
                    questText3 = questText3.Replace("（抓捕位置：", "");
                    questText3 = questText3.Replace("）", "");
                }
                string itemName = (string)Lang.GetItemName(itemID);
                if (finish)
                {
                    stringBuilder.Append($"[i:{itemID}]");
                    stringBuilder.Append("任务鱼: [c/00FF00:");
                    stringBuilder.Append(itemName);
                    stringBuilder.Append("]（");
                    stringBuilder.Append(questText3);
                    stringBuilder.Append("）");
                }
                else
                {
                    stringBuilder.Append($"[i:{itemID}]");
                    stringBuilder.Append("任务鱼: [c/FFFF00:");
                    stringBuilder.Append(itemName);
                    stringBuilder.Append("]（");
                    stringBuilder.Append(questText3);
                    stringBuilder.Append("）");
                }
            }
            else
            {
                stringBuilder.Append("[i:5275]任务鱼: [c/FF0000:渔夫不存在!]");
            }
            return stringBuilder.ToString();
        }

        public static bool CheckNPCActive(string npcId)
        {
            int result = 0;
            if (!int.TryParse(npcId, out result))
            {
                result = int.Parse(npcId);
            }
            for (int i = 0; i < 200; i++)
            {
                if (Main.npc[i].active && Main.npc[i].netID == result)
                {
                    return true;
                }
            }
            return false;
        }

        public static string GetEnvString(this TSPlayer plr)
        {
            StringBuilder stringBuilder = new StringBuilder();
            var envInfo = plr.GetEnvInfo();
            string envStr = (envInfo.Exists((string x) => x == "空岛") ? ("[c/00BFFF:" + string.Join(',', envInfo) + "]") : (envInfo.Exists((string x) => x == "地下") ? ("[c/FF8C00:" + string.Join(',', envInfo) + "]") : (envInfo.Exists((string x) => x == "洞穴") ? ("[c/A0522D:" + string.Join(',', envInfo) + "]") : ((!envInfo.Exists((string x) => x == "地狱")) ? ("[c/008000:" + string.Join(',', envInfo) + "]") : ("[c/FF0000:" + string.Join(',', envInfo) + "]")))));
            string envItem = (envInfo.Exists((string x) => x == "地牢") ? "[i:327]" : (envInfo.Exists((string x) => x == "腐化") ? "[i:942]" : (envInfo.Exists((string x) => x == "沙漠") ? "[i:910]" : (envInfo.Exists((string x) => x == "猩红") ? "[i:943]" : (envInfo.Exists((string x) => x == "神圣") ? "[i:944]" : (envInfo.Exists((string x) => x == "雪原") ? "[i:941]" : (envInfo.Exists((string x) => x == "丛林") ? "[i:940]" : (envInfo.Exists((string x) => x == "地狱") ? "[i:221]" : (envInfo.Exists((string x) => x == "神庙") ? "[i:1141]" : (envInfo.Exists((string x) => x == "海滩") ? "[i:4417]" : ((!envInfo.Exists((string x) => x == "发光蘑菇")) ? "[i:5245]" : "[i:183]")))))))))));
            stringBuilder.Append(envItem);
            stringBuilder.Append("当前环境:");
            stringBuilder.Append(envStr);
            return stringBuilder.ToString();
        }
        public static List<string> GetEnvInfo(this TSPlayer plr)
        {
            int index = plr.Index;
            List<string> list = new List<string>();
            if (Main.player[index].ZoneDungeon)
            {
                list.Add("地牢");
            }
            if (Main.player[index].ZoneCorrupt)
            {
                list.Add("腐化");
            }
            if (Main.player[index].ZoneHallow)
            {
                list.Add("神圣");
            }
            if (Main.player[index].ZoneMeteor)
            {
                list.Add("陨石");
            }
            if (Main.player[index].ZoneJungle)
            {
                list.Add("丛林");
            }
            if (Main.player[index].ZoneSnow)
            {
                list.Add("雪原");
            }
            if (Main.player[index].ZoneCrimson)
            {
                list.Add("猩红");
            }
            if (Main.player[index].ZoneWaterCandle)
            {
                list.Add("水蜡烛");
            }
            if (Main.player[index].ZonePeaceCandle)
            {
                list.Add("和平蜡烛");
            }
            if (Main.player[index].ZoneDesert)
            {
                list.Add("沙漠");
            }
            if (Main.player[index].ZoneGlowshroom)
            {
                list.Add("发光蘑菇");
            }
            if (Main.player[index].ZoneUndergroundDesert)
            {
                list.Add("地下沙漠");
            }
            if (Main.player[index].ZoneSkyHeight)
            {
                list.Add("空岛");
            }
            if (Main.player[index].ZoneDirtLayerHeight)
            {
                list.Add("地下");
            }
            if (Main.player[index].ZoneRockLayerHeight)
            {
                list.Add("洞穴");
            }
            if (Main.player[index].ZoneUnderworldHeight)
            {
                list.Add("地狱");
            }
            if (Main.player[index].ZoneBeach)
            {
                list.Add("海滩");
            }
            if (Main.player[index].ZoneRain)
            {
                list.Add("雨天");
            }
            if (Main.player[index].ZoneSandstorm)
            {
                list.Add("沙尘暴");
            }
            if (Main.player[index].ZoneGranite)
            {
                list.Add("花岗岩");
            }
            if (Main.player[index].ZoneMarble)
            {
                list.Add("大理石");
            }
            if (Main.player[index].ZoneHive)
            {
                list.Add("蜂巢");
            }
            if (Main.player[index].ZoneGemCave)
            {
                list.Add("宝石洞窟");
            }
            if (Main.player[index].ZoneLihzhardTemple)
            {
                list.Add("神庙");
            }
            if (Main.player[index].ZoneGraveyard)
            {
                list.Add("墓地");
            }
            if (Main.player[index].ZoneShadowCandle)
            {
                list.Add("阴影蜡烛");
            }
            if (Main.player[index].ZoneShimmer)
            {
                list.Add("微光");
            }
            if (Main.player[index].ShoppingZone_Forest)
            {
                list.Add("森林");
            }
            return list;
        }
        public static string GetLuck(this TSPlayer plr)
        {
            double num = Math.Round(Main.player[plr.Index].luck, 2,MidpointRounding.AwayFromZero);
            if (num < 0.0)
            {//保留两位小数
                return "[i:4478]幸运值:" + string.Format("{0:F2}",num).Color(PickColor(num)) + "点";
            }
            if (num == 0.0)
            {
                return "[i:4477]幸运值:" + string.Format("{0:F2}", num).Color(PickColor(num)) + "点";
            }
            return "[i:4479]幸运值:" + string.Format("{0:F2}", num).Color(PickColor(num)) + "点";
            static string PickColor(double luck)
            {

                double num2 = (luck + 0.585) / 1.17;
                int value = TShock.Utils.Clamp((int)(255.0 * num2), 255, 0);
                int value2 = TShock.Utils.Clamp((int)(255.0 * (1.0 - num2)), 255, 0);
                DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 3);
                defaultInterpolatedStringHandler.AppendFormatted(value2, "X2");
                defaultInterpolatedStringHandler.AppendFormatted(value, "X2");
                defaultInterpolatedStringHandler.AppendFormatted(0, "X2");
                return defaultInterpolatedStringHandler.ToStringAndClear();
            }
        }
        

    }
}
