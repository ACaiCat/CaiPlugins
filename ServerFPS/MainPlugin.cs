using System.Diagnostics;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace SBPlugin
{
    [ApiVersion(2, 1)]
    public class ServerFPS : TerrariaPlugin
    {

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
            //onleave钩子

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

        public static string ServerFPSText(double ServerFPS)
        {
            double Volatility = Math.Abs(ServerFPS- 60.0);
            ServerFPS = Math.Round(60 - Volatility,2, MidpointRounding.AwayFromZero);
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
