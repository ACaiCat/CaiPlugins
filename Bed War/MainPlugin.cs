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

namespace BedWar
{
    [ApiVersion(2, 1)]
    public class BedWar : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "起床战争";

        public override string Name => "BedWar";

        public override Version Version => new Version(1, 0, 0, 0);

        public BedWar(Main game)
        : base(game)
        {
        }

        public override void Initialize()
        {

            ServerApi.Hooks.DropBossBag.Register(this, test);

        }

     

        private void test(DropBossBagEventArgs args)
        {
            TSPlayer.All.SendSuccessMessage($"Npc: {Lang.GetNPCName(args.NpcId)} {args.NpcId} " +
                $"=> {TShock.Utils.ItemTag(new Item() { netID = args.ItemId, stack = args.Stack })}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //GetDataHandlers.NpcTalk.UnRegister(TalkNPC);
                ServerApi.Hooks.DropBossBag.Deregister(this, test);
            }
            base.Dispose(disposing);

        }
    }
}
