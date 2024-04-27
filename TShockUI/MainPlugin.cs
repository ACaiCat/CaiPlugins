using IL.Terraria.Chat.Commands;
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using OTAPI;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.GameContent.NetModules;
using Terraria.Net;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;
using System;
using System;
using System.Windows;
using HandyControl.Controls;

namespace TShockUI
{
    [ApiVersion(2, 1)]
    public class TShockUI : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "TShockUI";

        public override string Name => "TShockUI";

        public override Version Version => new Version(1, 0, 0, 0);

        public TShockUI(Main game)
        : base(game)
        {
        }

        public override void Initialize()
        {
            //Commands.ChatCommands.Add(new Command("tui", TuiCommand, "tui"));)
            

        }
        [STAThread]
        private void TuiCommand(CommandArgs args)
        {
            
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
