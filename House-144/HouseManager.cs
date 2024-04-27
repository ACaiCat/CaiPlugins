using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using TShockAPI;
using TShockAPI.DB;

namespace HousingPlugin;

public class HouseManager
{
	private const string cols = "Name, TopX, TopY, BottomX, BottomY, Author, Owners, WorldID, Locked, Users";

	public static bool AddHouse(int tx, int ty, int width, int height, string housename, string author)
	{
		int num = 1;
		if (HTools.GetHouseByName(housename) != null)
		{
			return false;
		}
		try
		{
			TShock.DB.Query("INSERT INTO HousingDistrict (Name, TopX, TopY, BottomX, BottomY, Author, Owners, WorldID, Locked, Users) VALUES (@0, @1, @2, @3, @4, @5, @6, @7, @8, @9);", housename, tx, ty, width, height, author, "", Main.worldID.ToString(), num, "");
		}
		catch (Exception ex)
		{
			TShock.Log.Error("房屋插件错误数据库写入错误:" + ex.ToString());
			return false;
		}
		HousingPlugin.Houses.Add(new House(new Rectangle(tx, ty, width, height), author, new List<string>(), housename, num == 1, new List<string>()));
		return true;
	}

	public static bool AddNewOwner(string houseName, string id)
	{
		House houseByName = HTools.GetHouseByName(houseName);
		if (houseByName == null)
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		houseByName.Owners.Add(id);
		for (int i = 0; i < houseByName.Owners.Count; i++)
		{
			string value = houseByName.Owners[i];
			num++;
			stringBuilder.Append(value);
			if (num != houseByName.Owners.Count)
			{
				stringBuilder.Append(",");
			}
		}
		try
		{
			string query = "UPDATE HousingDistrict SET Owners=@0 WHERE Name=@1";
			TShock.DB.Query(query, stringBuilder.ToString(), houseName);
		}
		catch (Exception ex)
		{
			TShock.Log.Error("房屋插件错误数据库修改错误:" + ex.ToString());
			return false;
		}
		return true;
	}

	public static bool AddNewUser(string houseName, string id)
	{
		House houseByName = HTools.GetHouseByName(houseName);
		if (houseByName == null)
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		houseByName.Users.Add(id);
		for (int i = 0; i < houseByName.Users.Count; i++)
		{
			string value = houseByName.Users[i];
			num++;
			stringBuilder.Append(value);
			if (num != houseByName.Users.Count)
			{
				stringBuilder.Append(",");
			}
		}
		try
		{
			string query = "UPDATE HousingDistrict SET Users=@0 WHERE Name=@1";
			TShock.DB.Query(query, stringBuilder.ToString(), houseName);
		}
		catch (Exception ex)
		{
			TShock.Log.Error("房屋插件错误数据库修改错误:" + ex.ToString());
			return false;
		}
		return true;
	}

	public static bool DeleteOwner(string houseName, string id)
	{
		House houseByName = HTools.GetHouseByName(houseName);
		if (houseByName == null)
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		houseByName.Owners.Remove(id);
		for (int i = 0; i < houseByName.Owners.Count; i++)
		{
			string value = houseByName.Owners[i];
			num++;
			stringBuilder.Append(value);
			if (num != houseByName.Owners.Count)
			{
				stringBuilder.Append(",");
			}
		}
		try
		{
			string query = "UPDATE HousingDistrict SET Owners=@0 WHERE Name=@1";
			TShock.DB.Query(query, stringBuilder.ToString(), houseName);
		}
		catch (Exception ex)
		{
			TShock.Log.Error("房屋插件错误数据库修改错误:" + ex.ToString());
			return false;
		}
		return true;
	}

	public static bool DeleteUser(string houseName, string id)
	{
		House houseByName = HTools.GetHouseByName(houseName);
		if (houseByName == null)
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		houseByName.Users.Remove(id);
		for (int i = 0; i < houseByName.Users.Count; i++)
		{
			string value = houseByName.Users[i];
			num++;
			stringBuilder.Append(value);
			if (num != houseByName.Users.Count)
			{
				stringBuilder.Append(",");
			}
		}
		try
		{
			string query = "UPDATE HousingDistrict SET Users=@0 WHERE Name=@1";
			TShock.DB.Query(query, stringBuilder.ToString(), houseName);
		}
		catch (Exception ex)
		{
			TShock.Log.Error("房屋插件错误数据库修改错误:" + ex.ToString());
			return false;
		}
		return true;
	}

	public static bool RedefineHouse(int tx, int ty, int width, int height, string housename)
	{
		try
		{
			House houseByName = HTools.GetHouseByName(housename);
			string name = houseByName.Name;
			try
			{
				string query = "UPDATE HousingDistrict SET TopX=@0, TopY=@1, BottomX=@2, BottomY=@3, WorldID=@4 WHERE Name=@5";
				TShock.DB.Query(query, tx, ty, width, height, Main.worldID.ToString(), houseByName.Name);
			}
			catch (Exception ex)
			{
				TShock.Log.Error("房屋插件错误数据库修改错误:" + ex.ToString());
				return false;
			}
			houseByName.HouseArea = new Rectangle(tx, ty, width, height);
		}
		catch (Exception ex2)
		{
			TShock.Log.Error("房屋插件错误重新定义房屋时出错:" + ex2.ToString());
			return false;
		}
		return true;
	}

	public static bool ChangeLock(House house)
	{
		if (house.Locked)
		{
			house.Locked = false;
		}
		else
		{
			house.Locked = true;
		}
		try
		{
			string query = "UPDATE HousingDistrict SET Locked=@0 WHERE Name=@1";
			TShock.DB.Query(query, house.Locked ? 1 : 0, house.Name);
		}
		catch (Exception ex)
		{
			TShock.Log.Error("房屋插件错误修改锁房屋时出错:" + ex.ToString());
			return false;
		}
		return true;
	}
}
