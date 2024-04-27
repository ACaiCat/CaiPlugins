using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace UserCheck
{
    [ApiVersion(2, 1)]
    public class HelpPlus : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "服务器内帮助";

        public override string Name => "ServerHelp";
        public override Version Version => new Version(1, 0, 0, 0);

        public HelpPlus(Main game)
        : base(game)
        {
            Order = int.MaxValue;
        }
        Command Command = new Command(Help, "服务器帮助");

        private static void Help(CommandArgs args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Player.SendSuccessMessage("服务器帮助列表:");
                foreach (var item in Config.config.HelpList)
                {
                    args.Player.SendSuccessMessage("[i:149][c/33ddce:" + item.Key + "]");
                }
                args.Player.SendSuccessMessage("[i:518]/服务器帮助 <帮助项>:");
                return;
            }
            if (args.Parameters[0] == "help")
            {
                args.Player.SendSuccessMessage("使用 /服务器帮助 <帮助项> 查看详细内容");
                foreach (var item in Config.config.HelpList)
                {
                    args.Player.SendSuccessMessage("[i:149][c/33ddce:" + item.Key+"]");
                }
                args.Player.SendSuccessMessage("[i:518]/服务器帮助 <帮助项>:");
                return;
            }
            if (Config.config.HelpList.ContainsKey(string.Join(" ", args.Parameters)))
            {
                args.Player.SendSuccessMessage("[i:518]" + string.Join(" ", args.Parameters) + "：");
                foreach (var item in Config.config.HelpList[string.Join(" ", args.Parameters)])
                {
                    args.Player.SendSuccessMessage("[c/33ddce:"+item+"]");
                }
                return;
            }
            else
            {
                args.Player.SendErrorMessage("未找到帮助项捏，下面是帮助项列表:");
                foreach (var item in Config.config.HelpList)
                {
                    args.Player.SendSuccessMessage("[i:149][c/33ddce:" + item.Key + "]");
                }
                args.Player.SendSuccessMessage("[i:518]使用 /服务器帮助 <帮助项> 查看详细内容");
                return;
            }
        }

        public override void Initialize()
        {
            Config.Read();
            GeneralHooks.ReloadEvent += GeneralHooks_ReloadEvent;
            Commands.ChatCommands.Add(Command);
            if (Config.config.RelaceRule)
            {
                Commands.ChatCommands.RemoveAll(x => x.Names.Contains("rules"));
                Commands.ChatCommands.Add(new Command(Help, "rules"));
            }
                

        }

        private void GeneralHooks_ReloadEvent(ReloadEventArgs e)
        {
            Config.Read();
            e.Player.SendSuccessMessage("[ServerHelp]插件配置已重载！");
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= GeneralHooks_ReloadEvent;
            }
            base.Dispose(disposing);
        }


    }
}
