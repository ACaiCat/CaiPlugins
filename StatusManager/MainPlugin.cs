using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TerrariaApi.Server;
using TShockAPI;
using System.Data;

namespace StatusMananger
{
    [ApiVersion(2, 1)]
    public class MainPlugin : TerrariaPlugin
    {
        public MainPlugin(Terraria.Main game) : base(game) { }

        public override string Name => "StatusTextMananger";

        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;

        public override string Author => "Cai";

        public override string Description => "Help you manage you status text";
        

        public override void Initialize()
        {
            ServerApi.Hooks.GamePostUpdate.Register(this, OnPostUpdate);
            ServerApi.Hooks.
            DBMananger.CreateTables();
        }

        public static IDbConnection DB { get; } = TShock.DB;

        private void OnPostUpdate(EventArgs args)
        {
            //try
            //{
            //    foreach (var tsplr in TShock.Players)
            //    {
            //        if (!this.isPlrSTVisible[tsplr.Index])
            //        {
            //            continue;
            //        }

            //        var sb = Utils.StringBuilderCache.Acquire();
            //        if (handlerList.Invoke(tsplr, sb, this.isPlrNeedInit[tsplr.Index]))
            //        {
            //            tsplr.SendData(PacketTypes.Status, Utils.StringBuilderCache.GetStringAndRelease(sb), 0, 0x1f);
            //            // 0x1f -> HideStatusTextPercent
            //        }
            //        else
            //        {
            //            Utils.StringBuilderCache.Release(sb);
            //        }
            //        this.isPlrNeedInit[tsplr.Index] = false;
            //    }

            //    Utils.Common.CountTick();
            //}
            //catch (Exception ex)
            //{
            //    Logger.Warn("Exception occur in OnGamePostUpdate, Ex: " + ex);
            //}
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {

                //ServerApi.Hooks.GamePostUpdate.Deregister(this, OnPostUpdate);
            }
            base.Dispose(disposing);
        }
    }
}
