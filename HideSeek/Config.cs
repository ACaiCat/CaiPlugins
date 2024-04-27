using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using TShockAPI;
using TShockAPI.DB;
using Region = TShockAPI.DB.Region;

namespace HideSeek
{
	// Token: 0x02000002 RID: 2
	public class Config
	{
		public const string Path = "tshock/HideSeek.json";

		public static Config config = new Config();
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

		public int GameLast = 300;

        public Point GameRoom = new(0,0); //游戏大厅
        //public Point CatRoom; //猫开始房间
        //public Point MouseRoom; //鼠开始房间 
        public Point CatStart = new(0, 0); //猫初始位置
        public Point MouseStart = new(0, 0);  //鼠初始位置
        public Dictionary<Point, int> ItemSpawn = new(); //物品生成点
		public string RegionName = "";
		public Region Region
		{
			get
			{
                return TShock.Regions.GetRegionByName(RegionName);
            }
		}
        public int MouseSSC = 0;
        public int CatSSC = 0;
        public int JoinSSC = 0;
		public int WinMoney = 200;
		public int LoseMoney =50;
    }
}
