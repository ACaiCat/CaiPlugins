using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace SwitchCommands
{
    public class HitSwitchEvent
    {
        //定义一个事件当HitSwitch时触发
        public static event EventHandler<HitSwitchEventArgs> HitSwitch;
        //定义一个事件参数
        public class HitSwitchEventArgs : EventArgs
        {
            public int X { get; set; }
            public int Y { get; set; }

            public TSPlayer TSPlayer { get; set; }
            public HitSwitchEventArgs(int x, int y,TSPlayer player)
            {
                X = x;
                Y = y;
                TSPlayer = player;
            }
        }
        //定义一个方法来触发事件
        public static void OnHitSwitch(int x, int y,TSPlayer player)
        {
            HitSwitch?.Invoke(null, new HitSwitchEventArgs(x, y,player));
        }

    }
}
