using MySqlX.XDevAPI.Common;
using System.Threading.Channels;
using TerrariaApi.Server;
using TShockAPI;

namespace Chireiden.TShock.Omni;

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
        player.SetData("chireiden.data.pingchannel", channel);
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
        player.SetData<Channel<int>?>("chireiden.data.pingchannel", null);
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
        var pingresponse = TShockAPI.TShock.Players[whoami]?.GetData<Channel<int>?>("chireiden.data.pingchannel");
        if (pingresponse == null)
        {
            return;
        }

        var index = BitConverter.ToInt16(args.Instance.readBuffer.AsSpan(args.ReadOffset, 2));
        pingresponse.Writer.TryWrite(index);
    }

    public static async Task<string> Command_Ping(TSPlayer plr)
    {
        try
        {
            var player = plr;
            var result = await Ping(player);
            if (result.TotalMilliseconds >= 200)
            {
                return ($"[c/FF0000:{result.TotalMilliseconds:F1}ms]");

            }
            else if (result.TotalMilliseconds >80 && result.TotalMilliseconds <200)
            {
                return ($"[c/FFA500:{result.TotalMilliseconds:F1}ms]");
            }
            else
            {
                return ($"[c/00FF00:{result.TotalMilliseconds:F1}ms]");
            }
        }
        catch (Exception e)
        {
            TShockAPI.TShock.Log.ConsoleError(e.ToString());
            return "[c/FF0000:不可用]";

        }
    }
}