using System.Reflection;
using On.OTAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace Test
{
    [ApiVersion(2, 1)]
    public class Test : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "test";

        public override string Name => "test";
        public override Version Version => new Version(1, 0, 0, 0);

        public Test(Main game)
        : base(game)
        {
            
        }

        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command(TestCmd, "test"));
        }

        private void TestCmd(CommandArgs args)
        {
            var player = args.Player;
            player.SendData(PacketTypes.PlayerSlot);


        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Commands.ChatCommands.RemoveAll(c => c.CommandDelegate.Method?.DeclaringType?.Assembly == Assembly.GetExecutingAssembly());
                //移除插件添加的命令
            }
            base.Dispose(disposing);
        }


    }
}
