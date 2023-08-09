using System.Drawing;
using System.IO.Streams;
using System.Text;
using SSCManager;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace LobbyManager
{
    [ApiVersion(2, 1)]
    public class LobbyManager : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "主城综合插件";

        public override string Name => "LobbyManager";

        public override Version Version => new Version(1, 0, 0, 0);
        public static PlayerData data { get; set; }
        public LobbyManager(Main game)
        : base(game)
        {
        }
        public static Config config { get; set; }
        public override void Initialize()
        {
            Config.GetConfig();
            RegionHooks.RegionLeft += RegionHooks_RegionLeft;
            PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            GeneralHooks.ReloadEvent+= OnReload;
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
            //当世界加载完成

        }


        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.SyncLoadout)
            {
                var plr = TShock.Players[args.Msg.whoAmI];
                plr.TPlayer.CurrentLoadoutIndex = 0;
                plr.SendData(PacketTypes.SyncLoadout, "", plr.Index);
                args.Handled = true;
                plr.SendWarningMessage("[i:5325]本服务器不允许切换装备栏!");
            }

        }

        private void OnReload(ReloadEventArgs e)
        {
            Config.GetConfig();
            e.Player.SendSuccessMessage("[i:50][主城综合]配置文件已重载!");


        }

  


        private void OnJoin(JoinEventArgs args)
        {
            if (TShock.Players[args.Who].HasPermission("lobby.ignore"))
            {
                return;
            }
            TShock.Players[args.Who].IgnoreSSCPackets = true;
        }

        private void PlayerHooks_PlayerPostLogin(PlayerPostLoginEventArgs e)
        {
            if (e.Player.HasPermission("lobby.ignore"))
            {
                return;
            }
            e.Player.RestoryBag(config.SSCId);
        }

        private void RegionHooks_RegionLeft(RegionHooks.RegionLeftEventArgs args)
        {
            if (args.Player.HasPermission("lobby.ignore"))
            {
                return;
            }
            args.Player.RestoryBag(config.SSCId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RegionHooks.RegionLeft -= RegionHooks_RegionLeft;
                PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;
                ServerApi.Hooks.ServerJoin.Deregister(this, OnJoin);
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }


    }
}
