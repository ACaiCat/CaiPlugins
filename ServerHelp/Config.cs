using System;
using System.Drawing;
using System.IO;
using System.Xml;
using Newtonsoft.Json;
using TShockAPI;
using TShockAPI.DB;
using Formatting = Newtonsoft.Json.Formatting;
using Region = TShockAPI.DB.Region;

namespace UserCheck
{
    public class Config
    {
        public const string Path = "tshock/ServerHelp.json";

        public static Config config = new Config();
        public Config()
        {
            HelpList = new Dictionary<string, List<string>>()
            {
                {"圈地" , new List<string>() {"服务器使用House圈地"} }
            };
        }
        public void Write(string path = Path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                this.Write(fileStream);
            }
        }

        public void Write(Stream stream)
        {
            string value = JsonConvert.SerializeObject(this, Formatting.Indented);
            using (StreamWriter streamWriter = new StreamWriter(stream))
            {
                streamWriter.Write(value);
            }
        }

        public static Config Read(string path = Path)
        {
            bool flag = !File.Exists(path);
            Config result;
            if (flag)
            {
                result = new Config();
                result.Write(path);
            }
            else
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    result = Config.Read(fileStream);
                }
            }
            config = result;
            return result;
        }

        public static Config Read(Stream stream)
        {
            Config result;
            using (StreamReader streamReader = new StreamReader(stream))
            {
                result = JsonConvert.DeserializeObject<Config>(streamReader.ReadToEnd());
            }
            return result;
        }
        [JsonProperty("帮助列表")]
        public Dictionary<string, List<string>> HelpList = new Dictionary<string, List<string>>();
        [JsonProperty("替换rules命令")]
        public bool RelaceRule = true;
    }
}
