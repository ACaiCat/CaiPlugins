using MySqlX.XDevAPI.Common;
using System.Threading.Channels;
using TerrariaApi.Server;
using TShockAPI;

namespace Ping;

public class PingClass
{
    public static async Task<TimeSpan> Ping(TSPlayer player, CancellationToken token = default)
    {
        var result = TimeSpan.MaxValue;

        var inv = -1;
        for (var i = 0; i < Terraria.Main.item.Length; i++)
        {
            if (!Terraria.Main.item[i].active || Terraria.Main.item[i].playerIndexTheItemIsReservedFor == 255)
            {
                inv = i;
                break;
            }
        }
        if (inv == -1)
        {
            return result;
        }

        var start = DateTime.Now;
        var channel = Channel.CreateBounded<int>(new BoundedChannelOptions(30)
        {
            SingleReader = true,
            SingleWriter = true
        });
        player.SetData("Ping.data.ping", channel);
        Terraria.NetMessage.TrySendData((int)PacketTypes.RemoveItemOwner, -1, -1, null, inv);
        while (!token.IsCancellationRequested)
        {
            var end = await channel.Reader.ReadAsync(token);
            if (end == inv)
            {
                result = DateTime.Now - start;
                break;
            }
        }
        player.SetData<Channel<int>?>("Ping.data.ping", null);
        return result;
    }

    public static void Hook_Ping_GetData(object? sender, OTAPI.Hooks.MessageBuffer.GetDataEventArgs args)
    {
        if (args.PacketId != (byte)PacketTypes.ItemOwner)
        {
            return;
        }

        var owner = args.Instance.readBuffer[args.ReadOffset + 2];
        if (owner != byte.MaxValue)
        {
            return;
        }

        var whoami = args.Instance.whoAmI;
        var pingresponse = TShock.Players[whoami]?.GetData<Channel<int>?>("Ping.data.ping");
        if (pingresponse == null)
        {
            return;
        }

        var index = BitConverter.ToInt16(args.Instance.readBuffer.AsSpan(args.ReadOffset, 2));
        pingresponse.Writer.TryWrite(index);
    }

    public static async Task<int> Command_Ping(TSPlayer plr)
    {
        try
        {
            var player = plr;
            var result = await Ping(player);
            return (int)result.TotalMilliseconds;
        }
        catch (Exception e)
        {
            TShock.Log.ConsoleError(e.ToString());
            return -1;

        }
    }
}