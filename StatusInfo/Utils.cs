using System;
using System.IO;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using TShockAPI;

namespace StatusInfo
{
    public static class Utils
    {
        public static int GetServerOnline(string adress,string token)
        {
            //写一个GET请求Github的API的例子
            //GET请求的参数直接拼接在URL后面
            string url = $"http://{adress}/v2/server/status?token={token}";
            //用于接收返回的JSON字符串
            string result = string.Empty;
            //实例化一个HTTP客户端
            HttpClient httpClient = new HttpClient();
            //发送GET请求
            bool success = true;
            Task.Run(async () =>
            {
                try
                {
                    result = await httpClient.GetStringAsync(url);

                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError("[全服在线]在线人数请求失败!!!\n" +
                        $"错误地址:{adress}");
                    success = false;
                }
            }).Wait();
            if (!success)
            {
                return 0;
            }
            dynamic json = JsonConvert.DeserializeObject(result);
            if (json["status"] == 200)
            {
                //TShock.Log.ConsoleInfo($"[全服在线]在线人数数值{json["playercount"]}\n地址:{adress}");
                return json["playercount"];
            }
            else
            {
                TShock.Log.ConsoleError("[全服在线]在线人数请求失败!可能是Token错误!\n" +
                    $"错误地址:{adress}");
                return 0;
            }

        }
    }

}
