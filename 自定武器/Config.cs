using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace 自定义武器
{

    public class Damage
    {
        [JsonProperty("最小值")]
        public int min = -1;
        [JsonProperty("最大值")]
        public int max = -1;
    }

    public class KnockBack
    {
        [JsonProperty("最小值")]
        public int min = -1;
        [JsonProperty("最大值")]
        public int max = -1;
    }

    public class Animation
    {
        [JsonProperty("最小值")]
        public int min = -1;
        [JsonProperty("最大值")]
        public int max = -1;
    }

    public class UseTime
    {
        [JsonProperty("最小值")]
        public int min = -1;
        [JsonProperty("最大值")]
        public int max = -1;
    }

    public class ShootSpeed
    {
        [JsonProperty("最小值")]
        public int min = -1;
        [JsonProperty("最大值")]
        public int max = -1;
    }

    public class Scale
    {
        [JsonProperty("最小值")]
        public int min = -1;
        [JsonProperty("最大值")]
        public int max = -1;
    }
    //knockBack useAnimation useTime shootSpeed scale

    public class Config
    {
        [JsonProperty("允许强化时间(秒)")]
        public int aQhSeconds = 30;
        [JsonProperty("强化有效时间(秒)")]
        public int qhSeconds = 300;
        [JsonProperty("伤害阈值")]
        public Damage Damage = new Damage();
        [JsonProperty("击退阈值")]
        public KnockBack KnockBack = new KnockBack();
        [JsonProperty("动画阈值")]
        public Animation Animation = new Animation();
        [JsonProperty("使用时间阈值")]
        public UseTime UseTime = new UseTime();
        [JsonProperty("射速阈值")]
        public ShootSpeed ShootSpeed = new ShootSpeed();
        [JsonProperty("比例阈值")]
        public Scale Scale = new Scale();
        public void Write(string path)
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

        public static Config Read(string path)
        {
            Config result;
            if (!File.Exists(path))
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
    }

}
