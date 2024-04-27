using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Plugin
{
    [ApiVersion(2, 1)]
    public class Plugin : TerrariaPlugin
    {
        //定义插件的作者名称
        public override string Author => "Cai";

        //插件的一句话描述
        public override string Description => "WeekdayLimit";

        //插件的名称
        public override string Name => "WeekdayLimit";

        //插件的版本
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        //插件的构造器
        public Plugin(Main game) : base(game)
        {
        }

        //插件加载时执行的代码
        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
        }

        public static bool IsWeekend()
        {
            DateTime now = DateTime.Now;
            return (now.DayOfWeek == DayOfWeek.Friday && now.Hour >= 17) ||
                   now.DayOfWeek == DayOfWeek.Saturday ||
                   now.DayOfWeek == DayOfWeek.Sunday ||
                   (now.DayOfWeek == DayOfWeek.Monday && now.Hour < 1);
        }
        private void OnJoin(JoinEventArgs args)
        {
            if (!IsWeekend())
            {
                TShock.Players[args.Who].Disconnect("本服务器只有在周五17点到周日之间才能加入游戏!");                
            }
        }

        //插件卸载时执行的代码

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //移除所有由本插件添加的所有指令
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                var asm = Assembly.GetExecutingAssembly();
                Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == asm);
            }
            base.Dispose(disposing);
        }
    }
}