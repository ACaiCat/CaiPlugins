using System.Diagnostics;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace VotePlus
{
    [ApiVersion(2, 1)]
    public class VotePlus : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "投票插件";

        public override string Name => "VotePlus";

        public override Version Version => new Version(1, 0, 0, 0);

        public VotePlus(Main game)
        : base(game)
        {
        }

        public static short Timer = 0;

        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);


        }

        private void OnUpdate(EventArgs args)
        {
            if (Timer == 60)
            {
                Timer = 0;
            }
            Timer++;
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
