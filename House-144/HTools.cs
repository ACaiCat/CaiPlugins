using System;
using System.Text.RegularExpressions;
using TShockAPI;
using TShockAPI.DB;

namespace HousingPlugin;

internal class HTools
{
	public static int MaxCount(TSPlayer ply)
	{
		for (int i = 0; i < ply.Group.permissions.Count; i++)
		{
			string input = ply.Group.permissions[i];
			Match match = Regex.Match(input, "^house\\.count\\.(\\d{1,9})$");
			if (match.Success)
			{
				return Convert.ToInt32(match.Groups[1].Value);
			}
		}
		return HousingPlugin.LConfig.最大房屋数量;
	}

	public static int MaxSize(TSPlayer ply)
	{
		for (int i = 0; i < ply.Group.permissions.Count; i++)
		{
			string input = ply.Group.permissions[i];
			Match match = Regex.Match(input, "^house\\.size\\.(\\d{1,9})$");
			if (match.Success)
			{
				return Convert.ToInt32(match.Groups[1].Value);
			}
		}
		return HousingPlugin.LConfig.最大房屋大小;
	}

	public static int MaxWidth(TSPlayer ply)
	{
		for (int i = 0; i < ply.Group.permissions.Count; i++)
		{
			string input = ply.Group.permissions[i];
			Match match = Regex.Match(input, "^house\\.width\\.(\\d{1,9})$");
			if (match.Success)
			{
				return Convert.ToInt32(match.Groups[1].Value);
			}
		}
		return HousingPlugin.LConfig.最大宽度;
	}

	public static int MaxHeight(TSPlayer ply)
	{
		for (int i = 0; i < ply.Group.permissions.Count; i++)
		{
			string input = ply.Group.permissions[i];
			Match match = Regex.Match(input, "^house\\.height\\.(\\d{1,9})$");
			if (match.Success)
			{
				return Convert.ToInt32(match.Groups[1].Value);
			}
		}
		return HousingPlugin.LConfig.最大高度;
	}

	public static House GetHouseByName(string name)
	{
		if (string.IsNullOrEmpty(name))
		{
			return null;
		}
		for (int i = 0; i < HousingPlugin.Houses.Count; i++)
		{
			House house = HousingPlugin.Houses[i];
			if (house != null && house.Name == name)
			{
				return house;
			}
		}
		return null;
	}

	public static bool OwnsHouse(UserAccount U, string housename)
	{
		if (U == null)
		{
			return false;
		}
		return OwnsHouse(U.ID.ToString(), housename);
	}

	public static bool OwnsHouse(UserAccount U, House house)
	{
		if (U == null)
		{
			return false;
		}
		return OwnsHouse(U.ID.ToString(), house);
	}

	public static bool OwnsHouse(string UserID, string housename)
	{
		if (string.IsNullOrWhiteSpace(UserID) || UserID == "0" || string.IsNullOrEmpty(housename))
		{
			return false;
		}
		House houseByName = GetHouseByName(housename);
		if (houseByName == null)
		{
			return false;
		}
		return OwnsHouse(UserID, houseByName);
	}

	public static bool OwnsHouse(string UserID, House house)
	{
		if (!string.IsNullOrEmpty(UserID) && UserID != "0" && house != null)
		{
			try
			{
				if (house.Owners.Contains(UserID))
				{
					return true;
				}
				return false;
			}
			catch (Exception ex)
			{
				TShock.Log.Error("房屋插件错误超标错误:" + ex.ToString());
				return false;
			}
		}
		return false;
	}

	public static bool CanUseHouse(string UserID, House house)
	{
		return !string.IsNullOrEmpty(UserID) && UserID != "0" && house.Users.Contains(UserID);
	}

	public static bool CanUseHouse(UserAccount U, House house)
	{
		return U != null && U.ID != 0 && house.Users.Contains(U.ID.ToString());
	}

	public static string InAreaHouseName(int x, int y)
	{
		for (int i = 0; i < HousingPlugin.Houses.Count; i++)
		{
			House house = HousingPlugin.Houses[i];
			if (house != null && x >= house.HouseArea.Left && x < house.HouseArea.Right && y >= house.HouseArea.Top && y < house.HouseArea.Bottom)
			{
				return house.Name;
			}
		}
		return null;
	}

	public static House InAreaHouse(int x, int y)
	{
		for (int i = 0; i < HousingPlugin.Houses.Count; i++)
		{
			House house = HousingPlugin.Houses[i];
			if (house != null && x >= house.HouseArea.Left && x < house.HouseArea.Right && y >= house.HouseArea.Top && y < house.HouseArea.Bottom)
			{
				return house;
			}
		}
		return null;
	}
}
