using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace PlaceholderAPI
{
    public class Hooks
    {
        public delegate void GetTextD(GetTextArgs args);
        public static event GetTextD PreGetText;
        public class GetTextArgs 
        {
            public Dictionary<string, string> List = new Dictionary<string, string>();
            public TSPlayer Player { get; set; }
            public GetTextArgs(Dictionary<string,string> list,TSPlayer plr)
            {
                List = list;
                Player = plr;
            }
        }
        public static void OnGetText(Dictionary<string, string> list,TSPlayer player) 
        {
            if (PreGetText == null)
            {
                return;
            }
            PreGetText(new GetTextArgs(list,player));
        }
    }
}
