using System.Drawing;
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
            base.Order = int.MinValue;
        }
        public static Config config { get; set; }
        public override void Initialize()
        {
            Config.GetConfig();
            TShockAPI.Hooks.RegionHooks.RegionLeft += RegionHooks_RegionLeft;
            TShockAPI.Hooks.PlayerHooks.PlayerPostLogin += PlayerHooks_PlayerPostLogin;
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            GeneralHooks.ReloadEvent+= OnReload;
            ServerApi.Hooks.NetGetData.Register(this, OnGetData);
        }

        private void OnGetData(GetDataEventArgs args)
        {
            if (args.MsgID == PacketTypes.SyncLoadout)
            {
                var plr = TShock.Players[args.Msg.whoAmI];
                plr.TPlayer.CurrentLoadoutIndex = 0;
                plr.SendData(PacketTypes.SyncLoadout, "", plr.Index);
                args.Handled = true;
            }
        }

        private void OnReload(ReloadEventArgs e)
        {
            Config.GetConfig();
            e.Player.SendSuccessMessage("[主城综合]配置玩家已重载!");
        }

        private void OnJoin(JoinEventArgs args)
        {
            if (TShock.Players[args.Who].HasPermission("lobby.ignore"))
            {
                return;
            }
            TShock.Players[args.Who].IgnoreSSCPackets = true;
        }

        private void PlayerHooks_PlayerPostLogin(TShockAPI.Hooks.PlayerPostLoginEventArgs e)
        {
            if (e.Player.HasPermission("lobby.ignore"))
            {
                return;
            }
            e.Player.RestoryBag(config.SSCId);
        }

        private void RegionHooks_RegionLeft(TShockAPI.Hooks.RegionHooks.RegionLeftEventArgs args)
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
                TShockAPI.Hooks.RegionHooks.RegionLeft -= RegionHooks_RegionLeft;
                TShockAPI.Hooks.PlayerHooks.PlayerPostLogin -= PlayerHooks_PlayerPostLogin;
                ServerApi.Hooks.ServerJoin.Register(this, OnJoin);
            }
            base.Dispose(disposing);
        }


    }
}
