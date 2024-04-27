using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using Terraria.DataStructures;
using TShockAPI.Hooks;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace HousingPlugin;

[ApiVersion(2, 1)]
public class HousingPlugin : TerrariaPlugin
{
    public static List<House> Houses = new List<House>();

    private static readonly System.Timers.Timer Update = new(1100.0);

    public static bool ULock = false;

    public override string Author => "GK 改良 &Cai";

    public override string Description => "一个著名的用于保护房屋的插件";

    public override string Name => "HousingDistricts";

    public override Version Version => new Version(1, 0, 1, 3);

    public static ConfigFile LConfig { get; set; }

    internal static string LConfigPath => Path.Combine(TShock.SavePath, "hconfig.json");

    public static LPlayer[] LPlayers { get; set; }

    public HousingPlugin(Main game)
        : base(game)
    {
        LPlayers = new LPlayer[256];
        base.Order = 5;
        LConfig = new ConfigFile();
    }

    private void RC()
    {
        try
        {
            if (!File.Exists(LConfigPath))
            {
                TShock.Log.ConsoleError("未找到房屋配置文件，已为你创建！修改配置后重启服务器可以应用新的配置");
            }
            LConfig = ConfigFile.Read(LConfigPath);
            LConfig.Write(LConfigPath);
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError("房屋插件错误配置读取错误:" + ex.ToString());
        }
    }

    private void RD()
    {
        SqlTable table = new SqlTable("HousingDistrict", new SqlColumn("ID", MySqlDbType.Int32)
        {
            Primary = true,
            AutoIncrement = true
        }, new SqlColumn("Name", MySqlDbType.VarChar, 255)
        {
            Unique = true
        }, new SqlColumn("TopX", MySqlDbType.Int32), new SqlColumn("TopY", MySqlDbType.Int32), new SqlColumn("BottomX", MySqlDbType.Int32), new SqlColumn("BottomY", MySqlDbType.Int32), new SqlColumn("Author", MySqlDbType.VarChar, 32), new SqlColumn("Owners", MySqlDbType.Text), new SqlColumn("WorldID", MySqlDbType.Text), new SqlColumn("Locked", MySqlDbType.Int32), new SqlColumn("Users", MySqlDbType.Text));
        IDbConnection dB = TShock.DB;
        IQueryBuilder provider;
        if (TShock.DB.GetSqlType() != SqlType.Sqlite)
        {
            IQueryBuilder queryBuilder = new MysqlQueryCreator();
            provider = queryBuilder;
        }
        else
        {
            IQueryBuilder queryBuilder2 = new SqliteQueryCreator();
            provider = queryBuilder2;
        }
        SqlTableCreator sqlTableCreator = new SqlTableCreator(dB, provider);
        sqlTableCreator.EnsureTableStructure(table);
    }

    private void RH()
    {
        QueryResult queryResult = TShock.DB.QueryReader("SELECT* FROM HousingDistrict");
        Houses.Clear();
        while (queryResult.Read())
        {
            if (!(queryResult.Get<string>("WorldID") != Main.worldID.ToString()))
            {
                string author = queryResult.Get<string>("Author");
                List<string> owners = queryResult.Get<string>("Owners").Split(',').ToList();
                bool locked = queryResult.Get<int>("Locked") == 1;
                List<string> users = queryResult.Get<string>("Users").Split(',').ToList();
                Houses.Add(new House(new Rectangle(queryResult.Get<int>("TopX"), queryResult.Get<int>("TopY"), queryResult.Get<int>("BottomX"), queryResult.Get<int>("BottomY")), author, owners, queryResult.Get<string>("Name"), locked, users));
            }
        }
    }

    public override void Initialize()
    {
        RC();
        RD();
        GetDataHandlers.InitGetDataHandler();
        GeneralHooks.ReloadEvent += GeneralHooks_ReloadEvent;
        ServerApi.Hooks.GameInitialize.Register(this, OnInitialize, int.MinValue);
        ServerApi.Hooks.NetGetData.Register(this, GetData, int.MinValue);
        ServerApi.Hooks.NetGreetPlayer.Register(this, OnGreetPlayer, int.MinValue);
        ServerApi.Hooks.ServerLeave.Register(this, OnLeave);
        ServerApi.Hooks.GamePostInitialize.Register(this, PostInitialize, int.MinValue);
        //钩子钩住 Liquid.AddWater(int,int)
        new Hook(typeof(Liquid).GetMethod("AddWater", new[] { typeof(int),typeof(int) })!, new Action<Action<int,int>, int,int>(AddWater));
    }

    private void AddWater(Action<int, int> action, int arg2, int arg3)
    {
        if (LConfig.禁止房屋液体流动&&HTools.InAreaHouse(arg2, arg3) != null)
        {
            return;
        }
        action(arg2, arg3);

    }

    private void GeneralHooks_ReloadEvent(ReloadEventArgs e)
    {
        RC();
        RD();
        e.Player.SendSuccessMessage("[House]房屋插件已重读!");
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ServerApi.Hooks.GameInitialize.Deregister(this, OnInitialize);
            ServerApi.Hooks.NetGetData.Deregister(this, GetData);
            ServerApi.Hooks.NetGreetPlayer.Deregister(this, OnGreetPlayer);
            ServerApi.Hooks.ServerLeave.Deregister(this, OnLeave);
            ServerApi.Hooks.GamePostInitialize.Deregister(this, PostInitialize);
            GeneralHooks.ReloadEvent -= GeneralHooks_ReloadEvent;
            Update.Elapsed -= OnUpdate;
            Update.Stop();
        }
        base.Dispose(disposing);
    }

    private void OnGreetPlayer(GreetPlayerEventArgs e)
    {
        lock (LPlayers)
        {
            LPlayers[e.Who] = new LPlayer(e.Who, TShock.Players[e.Who].TileX, TShock.Players[e.Who].TileY);
        }
    }

    private void OnLeave(LeaveEventArgs e)
    {
        lock (LPlayers)
        {
            if (LPlayers[e.Who] != null)
            {
                LPlayers[e.Who] = null;
            }
        }
    }

    private void OnInitialize(EventArgs args)
    {
        Commands.ChatCommands.Add(new Command("house.use", HCommands, "house")
        {
            HelpText = "输入/house help可以显示与房子相关的操作提示"
        });
    }

    public void PostInitialize(EventArgs e)
    {
        RH();
        Update.Elapsed += OnUpdate;
        Update.Start();
    }

    private void HCommands(CommandArgs args)
    {
        string text = "help";
        if (args.Parameters.Count > 0)
        {
            text = args.Parameters[0].ToLower();
        }
        if ((!args.Player.IsLoggedIn || args.Player.Account == null || args.Player.Account.ID == 0) && text != "help" && text != "delete" && text != "info" && text != "list" && text != "reset")
        {
            args.Player.SendErrorMessage("[i:4080]你必须登录才能使用房子插件");
            return;
        }
        switch (text)
        {
            case "name":
                args.Player.SendMessage("[i:4080]请敲击一个块查看它属于哪个房子", Color.Yellow);
                LPlayers[args.Player.Index].Look = true;
                break;
            case "set":
                {
                    int result = 0;
                    if (args.Parameters.Count == 2 && int.TryParse(args.Parameters[1], out result) && result >= 1 && result <= 2)
                    {
                        if (result == 1)
                        {
                            args.Player.SendMessage("[i:4080]现在请敲击要保护的区域的左上角", Color.Yellow);
                        }
                        if (result == 2)
                        {
                            args.Player.SendMessage("[i:4080]现在请敲击要保护的区域的右下角", Color.Yellow);
                        }
                        args.Player.AwaitingTempPoint = result;
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]指令错误! 正确指令: /house set [1/2]");
                    }
                    break;
                }
            case "tp":
                {
                    //检查是否有权限
                    if (!args.Player.HasPermission("house.tp"))
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限传送房屋!");
                        return;
                    }
                    if (args.Parameters.Count == 2)
                    {
                        House house = HTools.GetHouseByName(args.Parameters[1]);
                        if (house != null)
                        {
                            //检查是否拥有房屋或者被授权
                            if (!HTools.OwnsHouse(args.Player.Account,house.Name)&&!HTools.CanUseHouse(args.Player.Account, house)&&!args.Player.HasPermission("house.admin"))
                            {
                                args.Player.SendErrorMessage("[i:4080]你没有权限传送到这个房屋!");
                                return;
                            }
                            args.Player.Teleport(house.HouseArea.Center.X * 16, house.HouseArea.Center.Y * 16);

                            args.Player.SendSuccessMessage("[i:4080]你已被传送到房屋 {0}", house.Name);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]房屋不存在!");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]指令错误! 正确指令: /house tp <房屋名>");
                    }
                    break;
                }
                break;
            case "reset":
                {
                    if (!args.Player.HasPermission("house.admin"))
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限重置房屋插件!");
                        return;
                    }
                    string query = "DELETE FROM HousingDistrict";
                    TShock.DB.Query(query);
                    Houses.Clear();
                    args.Player.SendSuccessMessage("[i:4080]房屋插件已重置!");
                    break;
                }
            case "add":
                if (args.Parameters.Count > 1)
                {
                    int num = HTools.MaxCount(args.Player);
                    int num2 = 0;
                    for (int i = 0; i < Houses.Count; i++)
                    {
                        if (Houses[i].Author == args.Player.Account.ID.ToString())
                        {
                            num2++;
                        }
                    }
                    if (num2 < num || args.Player.HasPermission("house.bypasscount"))
                    {
                        if (!args.Player.TempPoints.Any((Point p) => p == Point.Zero))
                        {
                            string text2 = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                            if (string.IsNullOrEmpty(text2))
                            {
                                args.Player.SendErrorMessage("[i:4080]房屋名称不能为空");
                                break;
                            }
                            int num3 = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                            int num4 = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                            int num5 = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X) + 1;
                            int num6 = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y) + 1;
                            int num7 = HTools.MaxSize(args.Player);
                            int num8 = HTools.MaxWidth(args.Player);
                            int num9 = HTools.MaxHeight(args.Player);
                            if ((num5 * num6 <= num7 && num5 >= LConfig.最小宽度 && num6 >= LConfig.最小高度 && num5 <= num8 && num6 <= num9) || args.Player.HasPermission("house.bypasssize"))
                            {
                                Rectangle rectangle = new Rectangle(num3, num4, num5, num6);
                                for (int j = 0; j < Houses.Count; j++)
                                {
                                    if (rectangle.Intersects(Houses[j].HouseArea))
                                    {
                                        args.Player.TempPoints[0] = Point.Zero;
                                        args.Player.TempPoints[1] = Point.Zero;
                                        args.Player.SendErrorMessage("[i:4080]你选择的区域与其他房子存在重叠，这是不允许的");
                                        return;
                                    }
                                }
                                if (rectangle.Intersects(new Rectangle(Main.spawnTileX, Main.spawnTileY, TShock.Config.Settings.SpawnProtectionRadius, TShock.Config.Settings.SpawnProtectionRadius)))
                                {
                                    if (args.Player.HasPermission("house.ignorespawnpoint"))
                                    {
                                        args.Player.SendWarningMessage("[i:4080]你有管理权限,已为你忽略出生点保护!");
                                    }
                                    else
                                    {
                                        args.Player.TempPoints[0] = Point.Zero;
                                        args.Player.TempPoints[1] = Point.Zero;
                                        args.Player.SendErrorMessage("[i:4080]你选择的区域与出生保护范围重叠，这是不允许的");
                                        break;
                                    }


                                }
                                int num10 = 0;
                                int num11 = 0;
                                int num12 = 0;
                                int num13 = 0;
                                if (args.Player.TempPoints[0].X > args.Player.TempPoints[1].X)
                                {
                                    num10 = args.Player.TempPoints[1].X;
                                    num11 = args.Player.TempPoints[0].X;
                                }
                                else
                                {
                                    num10 = args.Player.TempPoints[0].X;
                                    num11 = args.Player.TempPoints[1].X;
                                }
                                if (num10 == num11)
                                {
                                    num11++;
                                }
                                if (args.Player.TempPoints[0].Y > args.Player.TempPoints[1].Y)
                                {
                                    num12 = args.Player.TempPoints[1].Y;
                                    num13 = args.Player.TempPoints[0].Y;
                                }
                                else
                                {
                                    num12 = args.Player.TempPoints[0].Y;
                                    num13 = args.Player.TempPoints[1].Y;
                                }
                                if (num12 == num13)
                                {
                                    num13++;
                                }
                                for (int k = num12; k <= num13; k++)
                                {
                                    for (int l = num10; l <= num11; l++)
                                    {
                                        ushort type = Main.tile[1, k].type;
                                        if (type != 0 && LConfig.禁止包含砖块.ContainsKey(type) && LConfig.禁止包含砖块.TryGetValue(type, out var value))
                                        {
                                            args.Player.TempPoints[0] = Point.Zero;
                                            args.Player.TempPoints[1] = Point.Zero;
                                            args.Player.SendErrorMessage("[i:4080]你选择的区域包含 {0} 砖块，这是不允许的", value);
                                            return;
                                        }
                                        type = Main.tile[l, k].wall;
                                        if (type != 0 && LConfig.禁止包含墙体.ContainsKey(type) && LConfig.禁止包含墙体.TryGetValue(type, out var value2))
                                        {
                                            args.Player.TempPoints[0] = Point.Zero;
                                            args.Player.TempPoints[1] = Point.Zero;
                                            args.Player.SendErrorMessage("[i:4080]你选择的区域包含 {0} 墙体，这是不允许的", value2);
                                            return;
                                        }
                                    }
                                }
                                for (int m = 0; m < TShock.Regions.Regions.Count; m++)
                                {
                                    if (rectangle.Intersects(TShock.Regions.Regions[m].Area))
                                    {
                                        args.Player.TempPoints[0] = Point.Zero;
                                        args.Player.TempPoints[1] = Point.Zero;
                                        args.Player.SendErrorMessage("[i:4080]你选择的区域与TShock区域 {0} 重叠，这是不允许的", TShock.Regions.Regions[m].Name);
                                        return;
                                    }
                                }
                                if (!HouseManager.AddHouse(num3, num4, num5, num6, text2, args.Player.Account.ID.ToString()))
                                {
                                    args.Player.SendErrorMessage("[i:4080]房子 " + text2 + " 已存在!");
                                    break;
                                }
                                args.Player.SendMessage("[i:4080]你建造了新房子 " + text2, Color.Yellow);
                                //Vector2 width = new Vector2(num5 * 16f, 0f);
                                //Vector2 height = new Vector2(0f, num6 * 16f);
                                ////Vector2 center = new Vector2(Area.Center.X * 16f, Area.Center.Y * 16f);
                                //int proj_ = Projectile.NewProjectile(new EntitySource_DebugCommand(), new Vector2(args.Player.TempPoints[0].X * 16f, args.Player.TempPoints[0].Y * 16f), width * 0.01f, 116, 0, 0f, 255, 0f, 0f);
                                //int proj_2 = Projectile.NewProjectile(new EntitySource_DebugCommand(), new Vector2(args.Player.TempPoints[0].X * 16f, args.Player.TempPoints[0].Y * 16f), height * 0.01f, 116, 0, 0f, 255, 0f, 0f);
                                //int proj_3 = Projectile.NewProjectile(new EntitySource_DebugCommand(), new Vector2((args.Player.TempPoints[0].X) * 16f, (args.Player.TempPoints[0].Y- height.Y + 1) * 16f), width * 0.01f, 116, 0, 0f, 255, 0f, 0f);
                                //int proj_4 = Projectile.NewProjectile(new EntitySource_DebugCommand(), new Vector2((args.Player.TempPoints[1].X + 1) * 16f, args.Player.TempPoints[1].Y +height.Y * 16f), height * 0.01f, 116, 0, 0f, 255, 0f, 0f);
                                ////int proj_5 = Projectile.NewProjectile(new EntitySource_DebugCommand(), center, Vector2.Zero, 467, 0, 0f, 255, 0f, 0f);
                                //args.Player.SendData(PacketTypes.ProjectileNew, "", proj_, 0f, 0f, 0f, 0);
                                //args.Player.SendData(PacketTypes.ProjectileNew, "", proj_2, 0f, 0f, 0f, 0);
                                //args.Player.SendData(PacketTypes.ProjectileNew, "", proj_3, 0f, 0f, 0f, 0);
                                //args.Player.SendData(PacketTypes.ProjectileNew, "", proj_4, 0f, 0f, 0f, 0);
                                ////TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", proj_5, 0f, 0f, 0f, 0);
                                TShock.Log.ConsoleInfo("[i:4080]{0} 建了新房子: {1}", args.Player.Account.Name, text2);
                            }
                            else
                            {
                                args.Player.SendErrorMessage("[i:4080]你设置的房屋宽:{0} 高:{1} 面积:{2} 需重新设置", num5, num6, num5 * num6);
                                if (num5 * num6 >= num7)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房子总面积超过了最大限制 " + num7 + " 格块");
                                }
                                if (num5 < LConfig.最小宽度)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋宽度小于最小限制 " + LConfig.最小宽度 + " 格块");
                                }
                                if (num6 < LConfig.最小高度)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋高度小于最小限制 " + LConfig.最小高度 + " 格块");
                                }
                                if (num5 > num8)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋宽度大于最大限制 " + num8 + " 格块");
                                }
                                if (num6 > num9)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋高度大于最大限制 " + num9 + " 格块");
                                }
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]未设置完整的房屋点,建议先使用指令: /house help");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]房屋添加失败:你只能添加{0}个房屋!", num);
                    }
                    args.Player.TempPoints[0] = Point.Zero;
                    args.Player.TempPoints[1] = Point.Zero;
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house add [屋名]");
                }
                break;
            case "allow":
                if (args.Parameters.Count > 2)
                {
                    string text10 = args.Parameters[1];
                    string name4 = string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2));
                    House houseByName8 = HTools.GetHouseByName(name4);
                    if (houseByName8 == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (houseByName8.Author == args.Player.Account.ID.ToString() || args.Player.HasPermission("house.admin"))
                    {
                        UserAccount userAccountByName4;
                        if ((userAccountByName4 = TShock.UserAccounts.GetUserAccountByName(text10)) != null)
                        {
                            if (LConfig.禁止分享所有者)
                            {
                                args.Player.SendErrorMessage("[i:4080]无法使用，因为服务器禁止了分享所有者功能");
                            }
                            else if (userAccountByName4.ID.ToString() != houseByName8.Author && !HTools.OwnsHouse(userAccountByName4.ID.ToString(), houseByName8))
                            {
                                if (HouseManager.AddNewOwner(houseByName8.Name, userAccountByName4.ID.ToString()))
                                {
                                    args.Player.SendMessage("[i:4080]成功为 " + text10 + " 添加房屋 " + houseByName8.Name + " 的拥有权!", Color.Yellow);
                                    TShock.Log.ConsoleInfo("[i:4080]{0} 添加 {1} 为房屋 {2} 的拥有者", args.Player.Account.Name, userAccountByName4.Name, houseByName8.Name);
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("[i:4080]添加用户权限失败");
                                }
                            }
                            else
                            {
                                args.Player.SendErrorMessage("[i:4080]用户 " + text10 + " 已拥有此房屋权限");
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]用户 " + text10 + " 不存在");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限分享这个房子!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house allow [名字] [屋名]");
                }
                break;
            case "disallow":
                if (args.Parameters.Count > 2)
                {
                    string text4 = args.Parameters[1];
                    House houseByName3 = HTools.GetHouseByName(string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2)));
                    if (houseByName3 == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (houseByName3.Author == args.Player.Account.ID.ToString() || args.Player.HasPermission("house.admin"))
                    {
                        UserAccount userAccountByName;
                        if ((userAccountByName = TShock.UserAccounts.GetUserAccountByName(text4)) != null)
                        {
                            if (!HTools.OwnsHouse(userAccountByName.ID.ToString(), houseByName3))
                            {
                                args.Player.SendErrorMessage("[i:4080]目标非此房屋拥有者");
                            }
                            else if (HouseManager.DeleteOwner(houseByName3.Name, userAccountByName.ID.ToString()))
                            {
                                args.Player.SendMessage("[i:4080]成功移除 " + text4 + " 的房屋 " + houseByName3.Name + "的拥有权!", Color.Yellow);
                                TShock.Log.ConsoleInfo("[i:4080]{0} 移除 {1} 的房屋 {2} 的拥有者", args.Player.Account.Name, userAccountByName.Name, houseByName3.Name);
                            }
                            else
                            {
                                args.Player.SendErrorMessage("[i:4080]移除用户权限失败");
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]用户 " + text4 + " 不存在");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限管理这个房子!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house disallow [名字] [屋名]");
                }
                break;
            case "adduser":
                if (args.Parameters.Count > 2)
                {
                    string text9 = args.Parameters[1];
                    string name3 = string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2));
                    House houseByName7 = HTools.GetHouseByName(name3);
                    if (houseByName7 == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (houseByName7.Author == args.Player.Account.ID.ToString() || args.Player.HasPermission("house.admin") || HTools.OwnsHouse(args.Player.Account, houseByName7))
                    {
                        UserAccount userAccountByName3;
                        if (houseByName7.Author != args.Player.Account.ID.ToString() && !args.Player.HasPermission("house.admin") && LConfig.禁止所有者修改使用者)
                        {
                            args.Player.SendErrorMessage("[i:4080]无法使用，因为服务器禁止了所有者分享使用者功能");
                        }
                        else if ((userAccountByName3 = TShock.UserAccounts.GetUserAccountByName(text9)) != null)
                        {
                            if (LConfig.禁止分享使用者)
                            {
                                args.Player.SendErrorMessage("[i:4080]无法使用，因为服务器禁止了分享使用者功能");
                            }
                            else if (userAccountByName3.ID.ToString() != houseByName7.Author && !HTools.OwnsHouse(userAccountByName3.ID.ToString(), houseByName7) && !HTools.CanUseHouse(userAccountByName3.ID.ToString(), houseByName7))
                            {
                                if (HouseManager.AddNewUser(houseByName7.Name, userAccountByName3.ID.ToString()))
                                {
                                    args.Player.SendMessage("[i:4080]成功为 " + text9 + " 添加房屋 " + houseByName7.Name + " 的使用权!", Color.Yellow);
                                    TShock.Log.ConsoleInfo("[i:4080]{0} 添加 {1} 为房屋 {2} 的使用者", args.Player.Account.Name, userAccountByName3.Name, houseByName7.Name);
                                }
                                else
                                {
                                    args.Player.SendErrorMessage("[i:4080]添加用户权限失败");
                                }
                            }
                            else
                            {
                                args.Player.SendErrorMessage("[i:4080]用户 " + text9 + " 已拥有此房屋权限");
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]用户 " + text9 + " 不存在");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限分享这个房子!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house adduser [名字] [屋名]");
                }
                break;
            case "deluser":
                if (args.Parameters.Count > 2)
                {
                    string text5 = args.Parameters[1];
                    House houseByName5 = HTools.GetHouseByName(string.Join(" ", args.Parameters.GetRange(2, args.Parameters.Count - 2)));
                    if (houseByName5 == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (houseByName5.Author == args.Player.Account.ID.ToString() || args.Player.HasPermission("house.admin") || HTools.OwnsHouse(args.Player.Account, houseByName5))
                    {
                        UserAccount userAccountByName2;
                        if (houseByName5.Author != args.Player.Account.ID.ToString() && !args.Player.HasPermission("house.admin") && LConfig.禁止所有者修改使用者)
                        {
                            args.Player.SendErrorMessage("[i:4080]无法使用，因为服务器禁止了所有者修改使用者功能");
                        }
                        else if ((userAccountByName2 = TShock.UserAccounts.GetUserAccountByName(text5)) != null)
                        {
                            if (!HTools.CanUseHouse(userAccountByName2.ID.ToString(), houseByName5))
                            {
                                args.Player.SendErrorMessage("[i:4080]目标非此房屋使用者");
                            }
                            else if (HouseManager.DeleteUser(houseByName5.Name, userAccountByName2.ID.ToString()))
                            {
                                args.Player.SendMessage("[i:4080]成功移除 " + text5 + " 的房屋 " + houseByName5.Name + "的使用权!", Color.Yellow);
                                TShock.Log.ConsoleInfo("[i:4080]{0} 移除 {1} 的房屋 {2} 的使用者", args.Player.Account.Name, userAccountByName2.Name, houseByName5.Name);
                            }
                            else
                            {
                                args.Player.SendErrorMessage("[i:4080]移除用户权限失败");
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]用户 " + text5 + " 不存在");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限管理这个房子!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house deluser [名字] [屋名]");
                }
                break;
            case "delete":
                if (args.Parameters.Count > 1)
                {
                    string name2 = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                    House houseByName4 = HTools.GetHouseByName(name2);
                    if (houseByName4 == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (houseByName4.Author == args.Player.Account.ID.ToString() || args.Player.HasPermission("house.admin"))
                    {
                        try
                        {
                            TShock.DB.Query("DELETE FROM HousingDistrict WHERE Name=@0", houseByName4.Name);
                        }
                        catch (Exception ex)
                        {
                            TShock.Log.Error("房屋插件错误删除错误" + ex.ToString());
                            args.Player.SendErrorMessage("[i:4080]房屋删除失败!");
                            break;
                        }
                        Houses.Remove(houseByName4);
                        args.Player.SendMessage("[i:4080]房屋: " + houseByName4.Name + " 删除成功!", Color.Yellow);
                        TShock.Log.ConsoleInfo("{0} 删除房屋: {1}", args.Player.Account.Name, houseByName4.Name);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限删除这个房子!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house delete [屋名]");
                }
                break;
            case "clear":
                args.Player.TempPoints[0] = Point.Zero;
                args.Player.TempPoints[1] = Point.Zero;
                args.Player.AwaitingTempPoint = 0;
                args.Player.SendMessage("[i:4080]临时敲击点清除完毕!", Color.Yellow);
                break;
            case "list":
                {
                    int result2 = 0;
                    DefaultInterpolatedStringHandler defaultInterpolatedStringHandler;
                    if (args.Parameters.Count > 1)
                    {
                        if (!int.TryParse(args.Parameters[1], out result2) || result2 < 1)
                        {
                            TSPlayer player = args.Player;
                            defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 1);
                            defaultInterpolatedStringHandler.AppendLiteral("无效页码 (");
                            defaultInterpolatedStringHandler.AppendFormatted(result2);
                            defaultInterpolatedStringHandler.AppendLiteral(")");
                            player.SendErrorMessage(defaultInterpolatedStringHandler.ToStringAndClear());
                            break;
                        }
                        result2--;
                    }
                    List<House> list = new List<House>();
                    for (int num24 = 0; num24 < Houses.Count; num24++)
                    {
                        if (args.Player.HasPermission("house.admin") || args.Player.Account.ID.ToString() == Houses[num24].Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), Houses[num24]) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), Houses[num24]))
                        {
                            list.Add(Houses[num24]);
                        }
                    }
                    if (list.Count == 0)
                    {
                        args.Player.SendMessage("[i:4080]你目前还没有已定义房屋", Color.Yellow);
                        break;
                    }
                    int num25 = list.Count / 15;
                    if (result2 > num25)
                    {
                        args.Player.SendErrorMessage("[i:4080]页码超过最大页数 ({0}/{1})", result2 + 1, num25 + 1);
                        break;
                    }
                    TSPlayer player2 = args.Player;
                    defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 2);
                    defaultInterpolatedStringHandler.AppendLiteral("[i:4080]目前的房屋 (");
                    defaultInterpolatedStringHandler.AppendFormatted(result2 + 1);
                    defaultInterpolatedStringHandler.AppendLiteral("/");
                    defaultInterpolatedStringHandler.AppendFormatted(num25 + 1);
                    defaultInterpolatedStringHandler.AppendLiteral(") 页:");
                    player2.SendMessage(defaultInterpolatedStringHandler.ToStringAndClear(), Color.Green);
                    List<string> list2 = new List<string>();
                    for (int num26 = result2 * 15; num26 < result2 * 15 + 15 && num26 < list.Count; num26++)
                    {
                        list2.Add(list[num26].Name);
                    }
                    string[] array = list2.ToArray();
                    for (int num27 = 0; num27 < array.Length; num27 += 5)
                    {
                        args.Player.SendMessage(string.Join(", ", array, num27, Math.Min(array.Length - num27, 5)), Color.Yellow);
                    }
                    if (result2 < num25)
                    {
                        TSPlayer player3 = args.Player;
                        defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
                        defaultInterpolatedStringHandler.AppendLiteral("输入 /house list ");
                        defaultInterpolatedStringHandler.AppendFormatted(result2 + 2);
                        defaultInterpolatedStringHandler.AppendLiteral(" 查看更多房屋");
                        player3.SendMessage(defaultInterpolatedStringHandler.ToStringAndClear(), Color.Yellow);
                    }
                    break;
                }
            case "redefine":
                if (args.Parameters.Count > 1)
                {
                    if (!args.Player.TempPoints.Any((Point p) => p == Point.Zero))
                    {
                        string text3 = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                        House houseByName2 = HTools.GetHouseByName(text3);
                        if (houseByName2 == null)
                        {
                            args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                            break;
                        }
                        if (houseByName2.Author == args.Player.Account.ID.ToString() || args.Player.HasPermission("house.admin"))
                        {
                            int num14 = Math.Min(args.Player.TempPoints[0].X, args.Player.TempPoints[1].X);
                            int num15 = Math.Min(args.Player.TempPoints[0].Y, args.Player.TempPoints[1].Y);
                            int num16 = Math.Abs(args.Player.TempPoints[0].X - args.Player.TempPoints[1].X) + 1;
                            int num17 = Math.Abs(args.Player.TempPoints[0].Y - args.Player.TempPoints[1].Y) + 1;
                            int num18 = HTools.MaxSize(args.Player);
                            int num19 = HTools.MaxWidth(args.Player);
                            int num20 = HTools.MaxHeight(args.Player);
                            if ((num16 * num17 <= num18 && num16 >= LConfig.最小宽度 && num17 >= LConfig.最小高度 && num16 <= num19 && num17 <= num20) || args.Player.HasPermission("house.bypasssize"))
                            {
                                Rectangle rectangle2 = new Rectangle(num14, num15, num16, num17);
                                for (int n = 0; n < Houses.Count; n++)
                                {
                                    if (rectangle2.Intersects(Houses[n].HouseArea) && Houses[n].Name != houseByName2.Name)
                                    {
                                        args.Player.TempPoints[0] = Point.Zero;
                                        args.Player.TempPoints[1] = Point.Zero;
                                        args.Player.SendErrorMessage("[i:4080]你选择的区域与其他房子存在重叠，这是不允许的");
                                        return;
                                    }
                                }
                                if (rectangle2.Intersects(new Rectangle(Main.spawnTileX, Main.spawnTileY, TShock.Config.Settings.SpawnProtectionRadius, TShock.Config.Settings.SpawnProtectionRadius)))
                                {
                                    args.Player.TempPoints[0] = Point.Zero;
                                    args.Player.TempPoints[1] = Point.Zero;
                                    args.Player.SendErrorMessage("[i:4080]你选择的区域与出生保护范围重叠，这是不允许的");
                                    break;
                                }
                                for (int num21 = 0; num21 < TShock.Regions.Regions.Count; num21++)
                                {
                                    if (rectangle2.Intersects(TShock.Regions.Regions[num21].Area))
                                    {
                                        args.Player.TempPoints[0] = Point.Zero;
                                        args.Player.TempPoints[1] = Point.Zero;
                                        args.Player.SendErrorMessage("[i:4080]你选择的区域与TShock区域 {0} 重叠，这是不允许的", TShock.Regions.Regions[num21].Name);
                                        return;
                                    }
                                }
                                if (!HouseManager.RedefineHouse(num14, num15, num16, num17, text3))
                                {
                                    args.Player.SendErrorMessage("[i:4080]重新定义房屋时出错!");
                                    break;
                                }
                                args.Player.SendMessage("[i:4080]重新定义了房子 " + text3, Color.Yellow);
                                TShock.Log.ConsoleInfo("[i:4080]{0} 重新定义的房子: {1}", args.Player.Account.Name, text3);
                            }
                            else
                            {
                                args.Player.SendErrorMessage("[i:4080]你设置的房屋宽:{0} 高:{1} 面积:{2} 需重新设置", num16, num17, num16 * num17);
                                if (num16 * num17 >= num18)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房子总面积超过了最大限制 " + num18 + " 格块");
                                }
                                if (num16 < LConfig.最小宽度)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋宽度小于最小限制 " + LConfig.最小宽度 + " 格块");
                                }
                                if (num17 < LConfig.最小高度)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋高度小于最小限制 " + LConfig.最小高度 + " 格块");
                                }
                                if (num16 > num19)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋宽度大于最大限制 " + num19 + " 格块");
                                }
                                if (num17 > num20)
                                {
                                    args.Player.SendErrorMessage("[i:4080]因为你的房屋高度大于最大限制 " + num20 + " 格块");
                                }
                            }
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]你没有权限修改这个房子!");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]未设置完整的房屋点,建议先使用指令: /house help");
                    }
                    args.Player.TempPoints[0] = Point.Zero;
                    args.Player.TempPoints[1] = Point.Zero;
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house redefine [屋名]");
                }
                break;
            case "info":
                if (args.Parameters.Count > 1)
                {
                    House houseByName6 = HTools.GetHouseByName(args.Parameters[1]);
                    if (houseByName6 == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (args.Player.HasPermission("house.admin") || args.Player.Account.ID.ToString() == houseByName6.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), houseByName6) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), houseByName6))
                    {
                        string text6 = "";
                        string text7 = "";
                        string text8 = "";
                        try
                        {
                            text8 = TShock.UserAccounts.GetUserAccountByID(Convert.ToInt32(houseByName6.Author)).Name;
                        }
                        catch (Exception ex2)
                        {
                            TShock.Log.Error("房屋插件错误超标错误:" + ex2.ToString());
                        }
                        for (int num22 = 0; num22 < houseByName6.Owners.Count; num22++)
                        {
                            string value3 = houseByName6.Owners[num22];
                            try
                            {
                                text6 = text6 + (string.IsNullOrEmpty(text6) ? "" : ", ") + TShock.UserAccounts.GetUserAccountByID(Convert.ToInt32(value3)).Name;
                            }
                            catch (Exception ex3)
                            {
                                TShock.Log.Error("房屋插件错误超标错误:" + ex3.ToString());
                            }
                        }
                        for (int num23 = 0; num23 < houseByName6.Users.Count; num23++)
                        {
                            string value4 = houseByName6.Users[num23];
                            try
                            {
                                text7 = text7 + (string.IsNullOrEmpty(text7) ? "" : ", ") + TShock.UserAccounts.GetUserAccountByID(Convert.ToInt32(value4)).Name;
                            }
                            catch (Exception ex4)
                            {
                                TShock.Log.Error("房屋插件错误超标错误:" + ex4.ToString());
                            }
                        }
                        args.Player.SendMessage("房屋 " + houseByName6.Name + " 的信息:", Color.LawnGreen);
                        args.Player.SendMessage("作者: " + text8, Color.LawnGreen);
                        args.Player.SendMessage("状态: " + ((!houseByName6.Locked || LConfig.禁止锁房屋) ? "未上锁" : "已上锁"), Color.LawnGreen);
                        args.Player.SendMessage("拥有者: " + text6, Color.LawnGreen);
                        args.Player.SendMessage("使用者: " + text7, Color.LawnGreen);
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限查看这个房子的信息!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house info [屋名]");
                }
                break;
            case "lock":
                if (args.Parameters.Count > 1)
                {
                    string name = string.Join(" ", args.Parameters.GetRange(1, args.Parameters.Count - 1));
                    House houseByName = HTools.GetHouseByName(name);
                    if (houseByName == null)
                    {
                        args.Player.SendErrorMessage("[i:4080]没有找到这个房子!");
                    }
                    else if (LConfig.禁止锁房屋)
                    {
                        args.Player.SendErrorMessage("[i:4080]无法使用，因为服务器关闭了锁房屋功能");
                    }
                    else if (args.Player.HasPermission("house.admin") || args.Player.Account.ID.ToString() == houseByName.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), houseByName) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), houseByName))
                    {
                        if (LConfig.禁止使用者改锁 && args.Player.Account.ID.ToString() != houseByName.Author && !HTools.OwnsHouse(args.Player.Account.ID.ToString(), houseByName) && HTools.CanUseHouse(args.Player.Account.ID.ToString(), houseByName) && !args.Player.HasPermission("house.admin"))
                        {
                            args.Player.SendErrorMessage("[i:4080]无法使用，因为服务器禁止了使用者改锁功能");
                        }
                        else if (HouseManager.ChangeLock(houseByName))
                        {
                            args.Player.SendMessage("[i:4080]房子: " + houseByName.Name + (houseByName.Locked ? " 上锁" : " 开锁"), Color.Yellow);
                            TShock.Log.ConsoleInfo("[i:4080]{0} 修改锁状态: {1}", args.Player.Account.Name, houseByName.Name);
                        }
                        else
                        {
                            args.Player.SendErrorMessage("[i:4080]修改房屋锁失败!");
                        }
                    }
                    else
                    {
                        args.Player.SendErrorMessage("[i:4080]你没有权限修改这个房子的锁!");
                    }
                }
                else
                {
                    args.Player.SendErrorMessage("[i:4080]语法错误! 正确语法: /house lock [屋名]");
                }
                break;
            default:
                args.Player.SendInfoMessage("[i:4080]创建房屋帮助:");
                args.Player.SendInfoMessage("[i:2703]输入[c/32FF82:/house set 1].");
                args.Player.SendInfoMessage("[i:2704]用[c/32FF82:镐子敲一下]要圈的地方的[c/32FF82:左上角].");
                args.Player.SendInfoMessage("[i:2705]输入[c/32FF82:/house set 2].");
                args.Player.SendInfoMessage("[i:2706]用[c/32FF82:镐子敲一下]要圈的地方的[c/32FF82:右下角].");
                args.Player.SendInfoMessage("[i:2707]输入[c/32FF82:/house add 房屋名字].");
                args.Player.SendInfoMessage("[i:472]输入[c/32FF82:/house lock 房屋名字]可以[c/32FF82:锁住房屋]哦.");
                args.Player.SendInfoMessage("其他房屋命令: list, allow, disallow, redefine, name, delete, clear, info, adduser, deluser, lock, reset.");
                break;
        }
    }

    private void GetData(GetDataEventArgs args)
    {
        if (args.Handled)
        {
            return;
        }
        TSPlayer tSPlayer = TShock.Players[args.Msg.whoAmI];
        if (tSPlayer == null)
        {
            args.Handled = true;
            return;
        }
        if (!tSPlayer.ConnectionAlive)
        {
            args.Handled = true;
            return;
        }
        using MemoryStream data = new MemoryStream(args.Msg.readBuffer, args.Index, args.Length);
        try
        {
            if (GetDataHandlers.HandlerGetData(args.MsgID, tSPlayer, data))
            {
                args.Handled = true;
            }
        }
        catch (Exception ex)
        {
            TShock.Log.Error("房屋插件错误传递时出错:" + ex.ToString());
        }
    }

    public void OnUpdate(object sender, ElapsedEventArgs e)
    {
        if (!LConfig.进出房屋提示 || ULock)
        {
            return;
        }
        ULock = true;
        DateTime now = DateTime.Now;
        lock (LPlayers)
        {
            LPlayer[] lPlayers = LPlayers;
            LPlayer[] array = lPlayers;
            foreach (LPlayer lPlayer in array)
            {
                if (lPlayer == null)
                {
                    continue;
                }
                if (Timeout(now))
                {
                    ULock = false;
                    return;
                }
                TSPlayer tSPlayer = TShock.Players[lPlayer.Who];
                if (tSPlayer == null)
                {
                    ULock = false;
                    return;
                }
                House house = HTools.InAreaHouse(tSPlayer.TileX, tSPlayer.TileY);
                House house2 = HTools.InAreaHouse(lPlayer.TileX, lPlayer.TileY);
                LPlayers[lPlayer.Who].TileX = tSPlayer.TileX;
                LPlayers[lPlayer.Who].TileY = tSPlayer.TileY;
                if (house2 == null && house == null)
                {
                    continue;
                }
                if (house2 == null && house != null)
                {
                    if (tSPlayer.Account.ID.ToString() == house.Author || HTools.OwnsHouse(tSPlayer.Account.ID.ToString(), house) || HTools.CanUseHouse(tSPlayer.Account.ID.ToString(), house))
                    {
                        tSPlayer.SendMessage("[i:4080]你进入了你的房子: " + house.Name, Color.LightSeaGreen);
                    }
                    else
                    {
                        tSPlayer.SendMessage("[i:4080]你进入了房子: " + house.Name, Color.LightSeaGreen);
                    }
                }
                else if (house2 != null && house == null)
                {
                    if (tSPlayer.Account.ID.ToString() == house2.Author || HTools.OwnsHouse(tSPlayer.Account.ID.ToString(), house2) || HTools.CanUseHouse(tSPlayer.Account.ID.ToString(), house2))
                    {
                        tSPlayer.SendMessage("[i:4080]你离开了你的房子: " + house2.Name, Color.LightSeaGreen);
                    }
                    else
                    {
                        tSPlayer.SendMessage("[i:4080]你离开了房子: " + house2.Name, Color.LightSeaGreen);
                    }
                }
                else if (!(house2.Name == house.Name))
                {
                    if (tSPlayer.Account.ID.ToString() == house.Author || HTools.OwnsHouse(tSPlayer.Account.ID.ToString(), house) || HTools.CanUseHouse(tSPlayer.Account.ID.ToString(), house))
                    {
                        tSPlayer.SendMessage("[i:4080]你进入了你的房子: " + house.Name, Color.LightSeaGreen);
                    }
                    else
                    {
                        tSPlayer.SendMessage("[i:4080]你进入了房子: " + house.Name, Color.LightSeaGreen);
                    }
                }
            }
        }
        ULock = false;
    }

    public static bool Timeout(DateTime Start, bool warn = true, int ms = 500)
    {
        bool flag = (DateTime.Now - Start).TotalMilliseconds >= (double)ms;
        if (flag)
        {
            ULock = false;
        }
        if (warn && flag)
        {
            TShock.Log.Error("房子插件提示处理超时,已抛弃部分提示!");
        }
        return flag;
    }
}
