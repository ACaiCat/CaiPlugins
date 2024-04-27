using System;
using System.IO;
using Terraria;
using TShockAPI;

namespace HousingPlugin;

public class GetDataHandlerArgs : EventArgs
{
	public TSPlayer Player { get; private set; }

	public MemoryStream Data { get; private set; }

	public Player TPlayer => Player.TPlayer;

	public GetDataHandlerArgs(TSPlayer player, MemoryStream data)
	{
		Player = player;
		Data = data;
	}
}
