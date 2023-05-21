using System;
using System.IO;
using Newtonsoft.Json;

namespace LobbyManager;

public class Config
{
	const string DefaultPath = "tshock/Lobby.json";
        public int SSCId { get; set; }
        public void Save()
	{
		using (StreamWriter streamWriter = new StreamWriter(DefaultPath))
		{
			streamWriter.WriteLine(JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}

	public static Config GetConfig()
	{
		Config config = new Config();
		bool flag = !File.Exists(DefaultPath);
		Config result;
		if (flag)
		{
			config.Save();
			result = config;
		}
		else
		{
			using (StreamReader streamReader = new StreamReader(DefaultPath))
			{
				config = JsonConvert.DeserializeObject<Config>(streamReader.ReadToEnd());
			}
			result = config;
		}
		LobbyManager.config = result;
		return result;
	}

}
