using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace HousingPlugin;

public class ConfigFile
{
	public bool 进出房屋提示 = true;

	public int 最大房屋大小 = 1000;

	public int 最小宽度 = 10;

	public int 最小高度 = 10;

	public int 最大宽度 = 100;

	public int 最大高度 = 100;

	public int 最大房屋数量 = 1;

	public bool 禁止锁房屋 = false;

	public bool 保护宝石锁 = false;

	public bool 始终保护箱子 = true;

	public bool 停用锁门 = false;

	public bool 冻结警告破坏者 = false;

	public bool 禁止分享所有者 = false;

	public bool 禁止分享使用者 = false;

	public bool 禁止所有者修改使用者 = true;

	public bool 禁止使用者改锁 = true;

	public bool 禁止房屋液体流动 = true;

	public Dictionary<int, string> 禁止包含砖块 = new Dictionary<int, string>();

	public Dictionary<int, string> 禁止包含墙体 = new Dictionary<int, string>();

	public static Action<ConfigFile> ConfigR;

	public static ConfigFile Read(string Path)
	{
		if (!File.Exists(Path))
		{
			ConfigFile configFile = new ConfigFile();
			configFile.禁止包含砖块 = new Dictionary<int, string> { { 226, "神庙砖" } };
			configFile.禁止包含墙体 = new Dictionary<int, string> { { 112, "神庙墙" } };
			return configFile;
		}
		using FileStream stream = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.Read);
		return Read(stream);
	}

	public static ConfigFile Read(Stream stream)
	{
		using StreamReader streamReader = new StreamReader(stream);
		ConfigFile configFile = JsonConvert.DeserializeObject<ConfigFile>(streamReader.ReadToEnd());
		if (ConfigR != null)
		{
			ConfigR(configFile);
		}
		return configFile;
	}

	public void Write(string Path)
	{
		using FileStream stream = new FileStream(Path, FileMode.Create, FileAccess.Write, FileShare.Write);
		Write(stream);
	}

	public void Write(Stream stream)
	{
		string value = JsonConvert.SerializeObject(this, Formatting.Indented);
		using StreamWriter streamWriter = new StreamWriter(stream);
		streamWriter.Write(value);
	}
}
