using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace TrPortRest;

[ApiVersion(2, 1)]
public class TrPortRest : TerrariaPlugin
{

    public override string Author => "Cai";

    public override string Description => "TrPortRest";

    public override string Name => "TrPortRest";

    public override Version Version => new Version(1, 0, 0, 0);

    public TrPortRest(Main game)
    : base(game)
    {
        Order = int.MaxValue;
    }

    public static MethodInfo onRequestMethod;
    public override void Initialize()
    {
        On.OTAPI.Hooks.MessageBuffer.InvokeGetData += MessageBuffer_InvokeGetData1;
        //Type restApiType = TShock.RestApi.GetType();

        //// 获取 OnRequest 方法的信息
        //// 假设 OnRequest 方法没有参数，如果有参数，你需要提供参数类型
        //onRequestMethod = restApiType.GetMethod("OnRequest", BindingFlags.NonPublic | BindingFlags.Instance);

        //// 确保找到了方法
        //if (onRequestMethod != null)
        //{
        //    try
        //    {
        //        // 调用 OnRequest 方法，传入 restApiInstance 作为调用该方法的实例
        //        // 由于 OnRequest 是私有方法，并且我们不知道它的具体签名，所以这里假设它没有参数

        //        // 假设没有参数绷不住了
        //        onRequestMethod.Invoke(TShock.RestApi, new object?[] { null, new RequestEventArgs() });
        //        Console.WriteLine("OnRequest method invoked successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        // 处理任何可能出现的异常
        //        Console.WriteLine("An error occurred: " + ex.Message);
        //    }
        //}
        //else
        //{
        //    Console.WriteLine("OnRequest method not found.");
        //}

    }
    static HttpClient httpClient = new();

    private bool MessageBuffer_InvokeGetData1(On.OTAPI.Hooks.MessageBuffer.orig_InvokeGetData orig, MessageBuffer instance, ref byte packetId, ref int readOffset, ref int start, ref int length, ref int messageType, int maxPackets)
    {
        if (messageType == 233)
        {
            
            instance.ResetReader();
            instance.reader.BaseStream.Position = start + 1;
             
            string guidID = instance.reader.ReadString();
            string message = instance.reader.ReadString();
            Task.Run(() =>
            {

                TShock.Log.ConsoleInfo($"[TrPortRest]收到请求:{message}");
                //TShock.RestApi.OnRequest
                //HttpClient httpClient = new();
                if (!TShock.Config.Settings.RestApiEnabled)
                {
                    TShock.Log.ConsoleError($"[TrPortRest]REST未启用!");
                    return;
                }
                var result =  httpClient.GetAsync($"http://127.0.0.1:{TShock.Config.Settings.RestApiPort}{message}").Result;
                MemoryStream memoryStream = new MemoryStream();
                var packetWriter = new BinaryWriter(memoryStream);
                packetWriter.BaseStream.Position = 2L;
                //messageType
                packetWriter.Write((byte)233);
                packetWriter.Write(guidID);
                packetWriter.Write(result.Content.ReadAsStringAsync().Result);
                //int num21 = (int)packetWriter.BaseStream.Position;
                //packetWriter.BaseStream.Position = 0L;
                //packetWriter.Write((ushort)num21);
                //packetWriter.BaseStream.Position = num21;
                var data = memoryStream.ToArray();
                System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(data, (ushort)data.Length);
                TShock.Players[instance.whoAmI].SendRawData(data);
            });

            //return false;
            return true;

        }
        return orig(instance, ref packetId, ref readOffset, ref start, ref length, ref messageType, maxPackets);
    }




    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            On.OTAPI.Hooks.MessageBuffer.InvokeGetData -= MessageBuffer_InvokeGetData1;


        }
        base.Dispose(disposing);
    }


}
