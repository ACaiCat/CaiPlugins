using System;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using TShockAPI;

namespace StatusInfo
{
    public class Config
    {
        [JsonProperty("计分板内容")]

        public string Content { get; set; } =
        "[c/66CCFF:「][c/55d284:喵][c/62d27a:窝][c/6fd16f:服][c/7cd165:务][c/89d15a:器][c/66CCFF:」]\r\n" +
        "[i:1182]地图名称:[c/00FF00:{地图名称}]\r\n" +
        "[i:17]时间:{当前时间}\r\n"+
        "[i:267]在线人数:[c/00FF00:{当前服务器在线人数}/{所有服务器总在线人数}]\r\n" +
        "[i:65]玩家名:[c/FFCC33:{玩家名}]\r\n" +
        "[i:306]当前组:[c/00BFFF:{玩家组名}]\r\n" +
        "{玩家手持物品.图标}手持物品:[c/00BFFF:{玩家手持物品.数量}X{玩家手持物品.修饰语}{玩家手持物品.名字}]\r\n" +
        "[i:29]生命:[c/009966:{玩家血量}/{玩家血量最大值}]\r\n" +
        "[i:109]魔力:[c/6699FF:{玩家魔力}/{玩家魔力最大值}]\r\n" +
        "[i:393]坐标:[c/FFFFFF:{玩家X坐标},{玩家Y坐标}]\r\n" +
        "[i:149]所处区域:[c/00BFFF:{玩家所处区域}]\r\n" +
        "[i:4344]发送数据包:{上一秒数据包发送}\r\n" +
        "[i:4344]接收数据包:{上一秒数据包接收}\r\n" +
        "[i:321]复活倒计时:[c/CCCCCC:{玩家死亡状态},{重生倒计时}秒]\r\n" +
            "{智能死亡时间}" +
        "[i:3122]Ping(延迟/平均):{玩家延迟}({全服平均延迟})\r\n" +
        "[i:3099]服务器帧率:{服务器帧率}\r\n" +
        "{当前环境}\r\n" +
        "{玩家幸运值}\r\n" +
        "{今日渔夫任务}\r\n";

        [JsonProperty("启用全服在线人数")]
        public bool EndableRestWho { get; set; } = true;

        [JsonProperty("刷新速度(毫秒)")]

        public int RefreshSpeed { get; set; } = 1000;

        [JsonProperty("服务器REST地址和Token对应表(不含本服)")]

        public Dictionary<string, string> Rests { get; set; } = new Dictionary<string, string>();

        public static Config Read(string savepath = "tshock/StatusInfo.json")
        {
            if (string.IsNullOrWhiteSpace(savepath))
            {
                savepath = TShock.SavePath;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(savepath));
            try
            {
                Config config = new Config();
                if (File.Exists(savepath))
                {
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(savepath));
                }
                else
                {
                    TShock.Log.ConsoleInfo("[StatusInfo]正在新建Config...");
                    File.WriteAllText(savepath, JsonConvert.SerializeObject(new Config(), Formatting.Indented));
                }
                return config;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return new Config();
            }
        }

        public bool Write(string savepath = "tshock/StatusInfo.json")
        {
            if (string.IsNullOrWhiteSpace(savepath))
            {
                savepath = TShock.SavePath;
            }
            Directory.CreateDirectory(Path.GetDirectoryName(savepath));
            try
            {
                File.WriteAllText(savepath, JsonConvert.SerializeObject(this, Formatting.Indented));
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return false;
            }
        }
    }
}
