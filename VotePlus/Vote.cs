using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoMod;
using TShockAPI;
using TShockAPI.DB;

namespace VotePlus
{
    public class Vote
    {
        public enum VoteType
        {
            PlayerKick, //踢出
            PlayerBan, //封禁 (感觉用不上)
            PlayerMute, //禁言
            BossClear, //清除BOSS(不掉落)
            EventClear, //关闭事件(不计为打败)
            NightRequest, //请求修改为夜晚 
            DayRequest, //请求修改为白天
            FreeVote //自由投票
        }
        public VoteType Type { get; set; }
        public UserAccount Sender { get; set; }

        public UserAccount Target { get; set; }

        public int BossID { get; set; }

        public string Project { get; set; }

        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndTime 
        {
            get 
            {
                return StartTime+Duration; 
            } 
        }
        public TimeSpan RemainTime
        {
            get
            {
                return EndTime - DateTime.Now; 
            } 
        }
        public bool IsEnd
        {
            get
            {
                return DateTime.Now > EndTime; 
            } 
        }
        public int Argeement { get; set; }
        public int Disargeement { get; set; }
        public int Total { get; set;}
        public int AgreePercent => (int)(Argeement / Total * 100);

        public short VoteReminder = 3;
        public string ReminderBuild(TimeSpan remain)
        {
            return "";
        }
        public void CheckReminder()
        {
            if (RemainTime.TotalSeconds <= 0)
            {
                return;
            }
            else if (RemainTime.TotalSeconds<=15 )
            {

            }
            else if (RemainTime.TotalSeconds>15 && RemainTime.TotalSeconds<=30)
            {
                
            }
            else if (RemainTime.TotalSeconds > 30 && RemainTime.TotalSeconds <= 45)
            {
                
            }
        }
    }
}
