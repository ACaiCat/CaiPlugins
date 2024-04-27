using System.Diagnostics;
using IL.Terraria.Utilities;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SBPlugin
{
    [ApiVersion(2, 1)]
    public class ServerFPS : TerrariaPlugin
    {
        private DateTime LastCheck;

        public override string Author => "Cai";

        public override string Description => "服务器FPS测试";

        public override string Name => "ServerFPS";

        public override Version Version => new Version(1, 0, 0, 0);

        public ServerFPS(Main game)
        : base(game)
        {
        }
        public static DateTime lastUpdate { get; set; } = DateTime.Now;
        public static double ServerFPSValues { get; set; } = 0;
        public static string GetServerFPS()
        {
            return ServerFPSText(ServerFPSValues);
        }
        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            ServerApi.Hooks.NetSendData.Register(this, OnSendData);
            //onleave钩子

        }

        //public static int cpu = 0; 
        //public static int memory = 0;
        //public static string upload = "";
        //public static string download = "";
        //private void Counter()
        //{
        //    try
        //    {
        //        CPUTime v1 = CPUHelper.GetCPUTime();
        //        var network = NetworkInfo.TryGetRealNetworkInfo();
        //        var oldRate = network.GetIpv4Speed();
        //        while (true)
        //        {
        //            Thread.Sleep(1000);
        //            //Stopwatch stopwatch = new Stopwatch();
        //            //stopwatch.Start();

        //            var v2 = CPUHelper.GetCPUTime();
        //            var value = CPUHelper.CalculateCPULoad(v1, v2);
        //            v1 = v2;

        //            var memoryInfo = MemoryHelper.GetMemoryValue();
        //            var newRate = network.GetIpv4Speed();
        //            var speed = NetworkInfo.GetSpeed(oldRate, newRate);
        //            oldRate = newRate;
        //            //stopwatch.Stop();
        //            cpu = (int)(value * 100);
        //            memory = (int)memoryInfo.UsedPercentage;
        //            upload = $"{speed.Sent.Size}{speed.Sent.SizeType}/S";
        //            download = $"{speed.Received.Size}{speed.Received.SizeType}/S";
        //            //Console.Clear();
        //            //Console.WriteLine($"CPU:    {(int)(value * 100)} %");
        //            //Console.WriteLine($"已用内存：{memoryInfo.UsedPercentage}%");
        //            //Console.WriteLine($"上传：{speed.Sent.Size} {speed.Sent.SizeType}/S    下载：{speed.Received.Size} {speed.Received.SizeType}/S");
        //            //Console.WriteLine($"{stopwatch.ElapsedMilliseconds}ms");
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e);
        //    }
        //}

        private void OnSendData(SendDataEventArgs args)
        {
            sendDataCount++;
        }

        int getDataCount = 0;
        int sendDataCount = 0;
        public static int lastGetDataCount { get; set; } = 0;
        public static int lastSendDataCount { get; set; } = 0;
        private void OnGetData(GetDataEventArgs args)
        {
            getDataCount++;
        }

        public static class StabilityStatistics
        {
            public static int FPSIndex { get; set; } = 0;
            public static double[] FPSArray { get; set; } = new double[60];

            public static double FPSFluctuation
            {
                get
                {
                    //把数组中的值全部加起来
                    double sum = 0;
                    for (int i = 0; i < 60; i++)
                    {
                        sum += FPSArray[i];
                    }
                    //求方差
                    double variance = 0;
                    for (int i = 0; i < 60; i++)
                    {
                        variance += (FPSArray[i] - 60) * (FPSArray[i] - 60);
                    }
                    variance = variance / 60;
                    //求标准差
                    double standardDeviation = Math.Sqrt(variance);
                    return Math.Round(standardDeviation);

                }
            }
            public static string FPSFluctuationText
            {
                get
                {
                    if (FPSFluctuation <= 30)
                    {
                        return $"[c/00FF00:{StabilityStatistics.FPSFluctuation}f]";
                    }

                    else if (FPSFluctuation > 30 && FPSFluctuation <= 100)
                    {

                        return $"[c/FFA500:{StabilityStatistics.FPSFluctuation}f]";
                    }
                    else if (FPSFluctuation > 100)
                    {
                        return $"[c/FF0000:{StabilityStatistics.FPSFluctuation}f]";
                    }
                    else
                    {
                        return $"[c/FF0000:不可用]";
                    }
                }
            }

        }




        //计算每秒游戏更新次数
        private void OnUpdate(EventArgs args)
        {
            if ((DateTime.UtcNow - LastCheck).TotalSeconds >= 1)
            {
                OnSecondUpdate();
                LastCheck = DateTime.UtcNow;
            }

            ServerFPSValues = 1 / (DateTime.Now - lastUpdate).TotalSeconds;
            StabilityStatistics.FPSArray[StabilityStatistics.FPSIndex] = ServerFPSValues;
            if (StabilityStatistics.FPSIndex == 59)
            {
                StabilityStatistics.FPSIndex = 0;
            }
            else
            {
                StabilityStatistics.FPSIndex++;
            }
            lastUpdate = DateTime.Now;
        }

        private void OnSecondUpdate()
        {
            lastGetDataCount = getDataCount;
            lastSendDataCount = sendDataCount;

            getDataCount = 0;
            sendDataCount = 0;
        }

        public static string ServerFPSText(double ServerFPS)
        {
            double Volatility = Math.Abs(ServerFPS - 60.0);
            ServerFPS = Math.Round(60 - Volatility, 2, MidpointRounding.AwayFromZero);
            if (Volatility <= 1)
            {
                return $"[c/00FF00:{string.Format("{0:F2}", ServerFPS)}fps]/{StabilityStatistics.FPSFluctuationText}";
            }

            else if (Volatility > 1 && Volatility <= 10)
            {

                return $"[c/FFA500:{string.Format("{0:F2}", ServerFPS)}fps]/{StabilityStatistics.FPSFluctuationText}";
            }
            else if (Volatility > 10)
            {
                return $"[c/FF0000:{string.Format("{0:F2}", ServerFPS)}fps]/{StabilityStatistics.FPSFluctuationText}";
            }
            else
            {
                return "[c/FF0000:不可用]";
            }
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

            }
            base.Dispose(disposing);
        }


    }
}
