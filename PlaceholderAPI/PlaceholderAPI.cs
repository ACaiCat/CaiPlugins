using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;

namespace PlaceholderAPI
{
    [ApiVersion(2,1)]
    public class PlaceholderAPI : TerrariaPlugin
    {
        public PlaceholderAPI(Terraria.Main game) : base(game){ Order = -1; }
        public override string Name => "PlaceholderAPI";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string Author => "豆沙";
        public override string Description => "一款通用占位符插件";
        public static PlaceholderAPI Instance { get { return instance; } }
        private static PlaceholderAPI instance;
        public PlaceholderManager placeholderManager = new PlaceholderManager();
        public override void Initialize()
        {
            instance = this;
            Register();
            placeholderManager.InitializeColors();
            Hooks.PreGetText += OnGetText;
        }
        private void OnGetText(Hooks.GetTextArgs args)
        {
            var plr = args.Player;
            args.List["{player}"] = plr.Name;
            args.List["{group}"] = plr.Group.Name;
            args.List["{helditem}"] = plr.TPlayer.HeldItem.netID.ToString();
            args.List["{playerDead}"] = (plr.Dead ? "已死亡" : "存活");
            args.List["{playerMaxHP}"] = plr.TPlayer.statLifeMax.ToString();
            args.List["{playerMaxMana}"] = plr.TPlayer.statManaMax.ToString();
            args.List["{playerHP}"] = plr.TPlayer.statLife.ToString();
            args.List["{playerMana}"] = plr.TPlayer.statMana.ToString();
            args.List["{region}"] = (plr.CurrentRegion == null ? "无" : plr.CurrentRegion.Name);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Hooks.PreGetText -= OnGetText;
            }
            base.Dispose(disposing);
        }
        private void Register() 
        {
            placeholderManager.Register("{player}");
            placeholderManager.Register("{group}");
            placeholderManager.Register("{helditem}");
            placeholderManager.Register("{playerDead}");
            placeholderManager.Register("{playerMaxHP}");
            placeholderManager.Register("{playerMaxMana}");
            placeholderManager.Register("{playerHP}");
            placeholderManager.Register("{playerMana}");
            placeholderManager.Register("{region}");
        }
    }
}
