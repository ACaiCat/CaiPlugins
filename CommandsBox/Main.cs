using System.IO.Streams;
using Microsoft.Xna.Framework;
using On.OTAPI;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Models.Projectiles;

namespace CommandsBox
{
    [ApiVersion(2, 1)]
    public class CommandsBox : TerrariaPlugin
    {

        public override string Author => "Cai";

        public override string Description => "将广播盒改造成命令方牌";

        public override string Name => "CommandsBox";

        public override Version Version => new Version(1, 0, 0, 0);

        public CommandsBox(Main game)
        : base(game)
        {
            Order = int.MaxValue;
        }

        public override void Initialize()
        {
            if (Terraria.Program.LaunchParameters.ContainsKey("-disableannouncementbox"))
            {
                TShock.Log.ConsoleWarn("[命令方牌]检测到本服务器禁用广播盒,本插件已禁用!\n如需启用请移除启动参数'-disableannouncentbox'");
                return;
            }
            else
            {
                TShock.Log.ConsoleWarn("[命令方牌]命令方牌已启用,将会替换广播盒作用!\n" +
                    "*本插件有提权风险,谨慎使用\n" +
                    "*编辑广播盒权限为: CommandsBox.Edit (不建议给普通用户)");

            }
            Hooks.Wiring.InvokeAnnouncementBox += OnAnnouncementBox;
            GetDataHandlers.SignRead.Register(OnSignReadText, priority: HandlerPriority.Lowest);
            //Hooks.MessageBuffer.InvokeGetData += OnGetData;
            ServerApi.Hooks.NetGetData.Register(this, OnGetData, int.MinValue);
            //GetDataHandlers.Sign.Register(OnSignChange);
            //BuffUpdate = new System.Timers.Timer { Interval = 1000, AutoReset = true, Enabled = true };
            //BuffUpdate.Elapsed += OnBuffUpdate;
            var position = new Vector2(0, 0);
            var velocity = new Vector2(3,33);
            int num = Projectile.NewProjectile(Projectile.GetNoneSource(), position, velocity, 1, 9999, 0);
        }

        private void OnGetData(GetDataEventArgs args)
        {
            
            if (args.MsgID == PacketTypes.SignNew)
            {
                if (args.Handled)
                {
                    return;
                }
                args.Handled = true;
                using (var data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length - 1))
                {
                    var whoami = args.Msg.whoAmI;
                    var signId = data.ReadInt16();
                    var x = data.ReadInt16();
                    var y = data.ReadInt16();
                    var text = data.ReadString();

                    if (signId >= 0 && signId < 1000)
                    {
                        
                        if (Main.tile[x, y].type == 425)
                        {
                            if (TShock.Players[whoami].Name!= TShock.Players[whoami].Account.Name)
                            {
                                TShock.Players[whoami].SendErrorMessage("[i:3617]用户名与游戏名不一致!");
                                return;
                            }

                            if (TShock.Players[whoami].HasPermission("CommandsBox.Edit"))
                            {
                                TShock.Players[whoami].SendSuccessMessage($"[i:3617]命令方牌将以[{TShock.Players[whoami].Account.Name}]的身份执行:\n" +
                                    $"{text}\n(一行一条命令)");
                                text = $"{TShock.Players[whoami].Account.Name}\n" + text;
                                
                            }
                            else
                            {
                                TShock.Players[whoami].SendErrorMessage("[i:3617]你没有权限编辑命令方牌!");
                                return;
                            }

                        }
                        string text2 = null;
                        if (Main.sign[signId] != null)
                        {
                            text2 = Main.sign[signId].text;
                        }
                        Main.sign[signId] = new Sign();
                        Main.sign[signId].x = x;
                        Main.sign[signId].y = y;
                        Sign.TextSign(signId, text);
                        if (text2 != text)
                        {
                            NetMessage.TrySendData(47, -1, whoami, null, signId, whoami);
                        }

                    }
                }
            }

        }

        //private bool OnGetData(Hooks.MessageBuffer.orig_InvokeGetData orig, MessageBuffer instance, ref byte packetId, ref int readOffset, ref int start, ref int length, ref int messageType, int maxPackets)
        //{


        //    return true;
        //}

        private void OnSignReadText(object? sender, GetDataHandlers.SignReadEventArgs e)
        {
            if (Main.tile[e.X, e.Y].type == 425)
            {
                var sign = Sign.ReadSign(e.X, e.Y);
                var text = Main.sign[sign].text.Split('\n').ToList();
                text.RemoveAt(0);
                e.Player.SendRawData(new RawDataBuilder(PacketTypes.SignNew).PackInt16(0).PackInt16((short)e.X).PackInt16((short)e.Y).PackString(string.Join('\n', text)).PackByte((byte)e.Player.Index).PackByte(new BitsByte(false)).GetByteData());
                e.Handled = true;

            }
        }

        private bool OnAnnouncementBox(Hooks.Wiring.orig_InvokeAnnouncementBox orig, int x, int y, int signId)
        {
            try
            {
                var text = Main.sign[signId].text;
                if (Wiring.CurrentUser != 255)
                {
                    text = Main.sign[signId].text.Replace("{name}", TShock.Players[Wiring.CurrentUser].Name);
                }
                var cmds = text.Split('\n').ToList();
                var plr = TShock.Players.Where(p => p!=null&&p.Active&&p.Account.Name == cmds[0]).ToList();
                if (plr.Count == 0)
                {
                    Group restPlayerGroup = TShock.Groups.GetGroupByName(TShock.UserAccounts.GetUserAccountByName(cmds[0]).Group);
                    if (restPlayerGroup == null)
                    {
                        return false;
                    }
                    TSRestPlayer tr = new TSRestPlayer(cmds[0], restPlayerGroup);
                    cmds.RemoveAt(0);

                    foreach (var cmd in cmds)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cmd))
                            {
                                continue;
                            }

                            Commands.HandleCommand(tr, cmd);
                        }
                        catch (Exception ex)
                        {
                            TShock.Log.ConsoleError(ex.ToString());
                        }


                    }
                }
                else
                {
                    cmds.RemoveAt(0);
                    foreach (var cmd in cmds)
                    {
                        try
                        {
                            if (string.IsNullOrEmpty(cmd))
                            {
                                continue;
                            }
                            Commands.HandleCommand(plr[0], cmd);
                        }
                        catch (Exception ex)
                        {
                            TShock.Log.ConsoleError(ex.ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
            return false;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Hooks.Wiring.InvokeAnnouncementBox -= OnAnnouncementBox;
                GetDataHandlers.SignRead.UnRegister(OnSignReadText);
                ServerApi.Hooks.NetGetData.Deregister(this, OnGetData);
            }
            base.Dispose(disposing);
        }


    }
}
