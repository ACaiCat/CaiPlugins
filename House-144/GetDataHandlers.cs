using System.IO.Streams;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Tile_Entities;
using Terraria.Localization;
using TShockAPI;
using System.Collections.Generic;
using OTAPI;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Achievements;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ObjectData;
using MySqlX.XDevAPI;
using static MonoMod.InlineRT.MonoModRule;

namespace HousingPlugin;


public static class GetDataHandlers
{
    private static string EditHouse = "house.edit";

    private static Dictionary<PacketTypes, GetDataHandlerDelegate> GetDataHandlerDelegates;

    public static void InitGetDataHandler()
    {
      
        Dictionary<PacketTypes, GetDataHandlerDelegate> dictionary = new Dictionary<PacketTypes, GetDataHandlerDelegate>
        {
            { PacketTypes.Tile, HandleTile },
            { PacketTypes.DoorUse, HandleDoorUse },
            { PacketTypes.TileSendSquare, HandleSendTileSquareCentered },
            { PacketTypes.ChestGetContents, HandleChestOpen },
            { PacketTypes.ChestItem, HandleChestItem },
            { PacketTypes.ChestOpen, HandleChestActive },
            { PacketTypes.PlaceChest, HandlePlaceChest },
            { PacketTypes.SignNew, HandleSign },
            { PacketTypes.LiquidSet, HandleLiquidSet },
            { PacketTypes.PaintTile, HandlePaintTile },
            { PacketTypes.PaintWall, HandlePaintWall },
            { PacketTypes.PlaceObject, HandlePlaceObject },
            { PacketTypes.PlaceTileEntity, HandlePlaceTileEntity },
            { PacketTypes.PlaceItemFrame, HandlePlaceItemFrame },
            { PacketTypes.GemLockToggle, HandleGemLockToggle },
            { PacketTypes.MassWireOperation, HandleMassWireOperation },
            { PacketTypes.RequestTileEntityInteraction, HandleRequestTileEntityInteraction },
            { PacketTypes.WeaponsRackTryPlacing, HandleWeaponsRackTryPlacing },
            { PacketTypes.ChestName, HandleEditChestName },
            { PacketTypes.ForceItemIntoNearestChest, HandleForceItemIntoNearestChest }
        };
        GetDataHandlerDelegates = dictionary;
    }

    public static bool CheckNearestChest(GetDataHandlerArgs args)
    {
        bool flag5 = false;
        bool flag6 = true;
        int plr = args.TPlayer.whoAmI;
        int slot = args.Data.ReadInt16();
        Item item = null;
        Vector2 position = Main.player[plr].Center;
        int playerID = plr;
        if (slot >= PlayerItemSlotID.Bank4_0 && slot < PlayerItemSlotID.Bank4_0 + 40)
        {
            int num = slot - PlayerItemSlotID.Bank4_0;
            item = Main.player[plr].bank4.item[num];
            flag6 = true;
        }
        else if (slot < 58)
        {
            item = Main.player[plr].inventory[slot];
            flag6 = false;
        }

        bool flag = true;
        for (int i = 0; i < 8000; i++)
        {
            bool flag2 = false;
            bool flag3 = false;
            if (Main.chest[i] == null || Chest.IsPlayerInChest(i) || Chest.IsLocked(Main.chest[i].x, Main.chest[i].y))
            {
                continue;
            }

            Vector2 vector = new Vector2(Main.chest[i].x * 16 + 16, Main.chest[i].y * 16 + 16);
            if ((vector - position).Length() >= 600f || !Hooks.Chest.InvokeQuickStack(playerID, item, i))
            {
                continue;
            }

            for (int j = 0; j < Main.chest[i].item.Length; j++)
            {
                if (Main.chest[i].item[j].IsAir)
                {
                    flag3 = true;
                }
                else if (item.IsTheSameAs(Main.chest[i].item[j]))
                {
                    flag2 = true;
                    int num = Main.chest[i].item[j].maxStack - Main.chest[i].item[j].stack;
                    if (num > 0)
                    {
                        int x = Main.chest[i].x;
                        int y = Main.chest[i].y;
                        House house = HTools.InAreaHouse(x, y);
                        if (house == null)
                        {
                            continue;
                        }
                        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
                        {
                            continue;
                        }
                        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
                        {
                            continue;
                        }
                        flag5 = true;
                    }
                }
                else
                {
                    flag3 = true;
                }
            }

            if (!(flag2 && flag3) || item.stack <= 0)
            {
                continue;
            }

            for (int k = 0; k < Main.chest[i].item.Length; k++)
            {
                if (Main.chest[i].item[k].type == 0 || Main.chest[i].item[k].stack == 0)
                {
                    if (flag)
                    {
                        int x = Main.chest[i].x;
                        int y = Main.chest[i].y;
                        House house = HTools.InAreaHouse(x, y);
                        if (house == null)
                        {
                            continue;
                        }
                        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
                        {
                            continue;
                        }
                        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
                        {
                            continue;
                        }
                        flag5 = true;
                    }
                }
            }
        }
        if (flag5)
        {
            if (flag6)
            {
                
                int num = slot - PlayerItemSlotID.Bank4_0;
                NetMessage.SendData(5, -1, -1, null, plr, slot, (int)Main.player[plr].bank4.item[num].prefix);
            }
            else
            {
                NetMessage.SendData(5, -1, -1, null, plr, slot, (int)Main.player[plr].inventory[slot].prefix);
            }
        }
        return flag5;
    }

    private static bool HandleForceItemIntoNearestChest(GetDataHandlerArgs args)
    {
        bool flag = CheckNearestChest(args);
        if (flag)
        {
            if (HousingPlugin.LConfig.冻结警告破坏者)
            {
                args.Player.Disable("[i:511]你无权将物品堆叠至此箱子!");
            }
            if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
            {
                args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

            }
            else
            {
                return true;
            }
            args.Player.SendErrorMessage("[i:511]你没有权力将物品堆叠至此箱子!");
        }
        return flag;
    }

    private static bool HandleEditChestName(GetDataHandlerArgs args)
    {
        int num = args.Data.ReadInt16();
        int x = Main.chest[num].x;
        int y = Main.chest[num].y;
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改箱子名字!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改箱子的名字!");
        return true;
    }

    public static bool HandlerGetData(PacketTypes type, TSPlayer player, MemoryStream data)
    {
        if (GetDataHandlerDelegates.TryGetValue(type, out var value))
        {
            try
            {
                return value(new GetDataHandlerArgs(player, data));
            }
            catch (Exception ex)
            {
                TShock.Log.Error("房屋插件错误调用事件时出错:" + ex.ToString());
            }
        }
        return false;
    }

    private static bool HandleTile(GetDataHandlerArgs args)
    {
        int num = args.Data.ReadInt8();
        int num2 = args.Data.ReadInt16();
        int num3 = args.Data.ReadInt16();
        ITile val = Main.tile[num2, num3];
        //if (Main.tileCut[val.type])
        //{
        //	return false;
        //}
        House house = HTools.InAreaHouse(num2, num3);
        if (HousingPlugin.LPlayers[args.Player.Index].Look)
        {
            if (house == null)
            {
                args.Player.SendMessage("[i:3620]敲击处不属于任何房子!", Color.Yellow);
            }
            else
            {
                string text = "";
                try
                {
                    text = TShock.UserAccounts.GetUserAccountByID(Convert.ToInt32(house.Author)).Name;
                }
                catch (Exception ex)
                {
                    TShock.Log.Error("房屋插件错误超标错误:" + ex.ToString());
                }
                args.Player.SendMessage("[i:3620]敲击处为 " + text + " 的房子: " + house.Name + " 状态: " + ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) ? "未上锁" : "已上锁"), Color.Yellow);
            }
            args.Player.SendTileSquareCentered(num2, num3);
            HousingPlugin.LPlayers[args.Player.Index].Look = false;
            return true;
        }
        if (args.Player.AwaitingTempPoint > 0)
        {
            args.Player.TempPoints[args.Player.AwaitingTempPoint - 1].X = num2;
            args.Player.TempPoints[args.Player.AwaitingTempPoint - 1].Y = num3;
            if (args.Player.AwaitingTempPoint == 1)
            {
                args.Player.SendMessage("[i:851]保护区左上角已设置!", Color.Yellow);
            }
            if (args.Player.AwaitingTempPoint == 2)
            {
                args.Player.SendMessage("[i:850]保护区右下角已设置!", Color.Yellow);
            }
            args.Player.SendTileSquareCentered(num2, num3);
            args.Player.AwaitingTempPoint = 0;
            return true;
        }
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        args.Player.SendTileSquareCentered(num2, num3);
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]无权修改房子保护!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权限损坏被房子保护的地区!");

        return true;
    }

    private static bool HandleDoorUse(GetDataHandlerArgs args)
    {
        args.Data.ReadInt8();
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (!house.Locked || HousingPlugin.LConfig.禁止锁房屋 || HousingPlugin.LConfig.停用锁门)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        args.Player.SendTileSquareCentered(x, y);
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改门!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }

        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护的地区的门!");
        return true;
    }

    private static bool HandleSendTileSquareCentered(GetDataHandlerArgs args)
    {
        return false;
    }

    private static bool HandleChestOpen(GetDataHandlerArgs args)
    {
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权打开箱子!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力打开被房子保护的地区的箱子!");
        return true;
    }

    private static bool HandleChestItem(GetDataHandlerArgs args)
    {
        int num = args.Data.ReadInt16();
        int x = Main.chest[num].x;
        int y = Main.chest[num].y;
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权更新箱子!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力更新被房子保护的地区的箱子!");
        return true;
    }

    private static bool HandleChestActive(GetDataHandlerArgs args)
    {
        args.Data.ReadInt16();
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改箱子!");
        }
        args.Player.SendData(PacketTypes.ChestOpen, "", -1);
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护的地区的箱子!");
        return true;
    }

    private static bool HandlePlaceChest(GetDataHandlerArgs args)
    {
        args.Data.ReadByte();
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        Rectangle value = new Rectangle(x, y, 3, 3);
        for (int i = 0; i < HousingPlugin.Houses.Count; i++)
        {
            House house = HousingPlugin.Houses[i];
            if (house != null && house.HouseArea.Intersects(value) && !args.Player.Group.HasPermission(EditHouse) && !(args.Player.Account.ID.ToString() == house.Author) && !HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
            {
                if (HousingPlugin.LConfig.冻结警告破坏者)
                {
                    args.Player.Disable("[i:511]你无权放置家具!");
                }
                args.Player.SendTileSquareCentered(x, y, 3);
                if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
                {
                    args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

                }
                else
                {
                    return true;
                }
                args.Player.SendErrorMessage("[i:511]你没有权力放置被房子保护的地区的家具!");
                
                return true;
            }
        }
        return false;
    }

    private static bool HandleSignRead(GetDataHandlerArgs args)
    {
        return false;
    }

    private static bool HandleSign(GetDataHandlerArgs args)
    {
        short number = args.Data.ReadInt16();
        short x = args.Data.ReadInt16();
        short y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改标牌!");
        }
        args.Player.SendData(PacketTypes.SignNew, "", number);
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护的地区的标牌!");
        return true;
    }

    private static bool HandleLiquidSet(GetDataHandlerArgs args)
    {
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权放水!");
        }
        args.Player.SendTileSquareCentered(x, y);
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力在被房子保护的地区放水!");

        return true;
    }

    private static bool HandleHitSwitch(GetDataHandlerArgs args)
    {
        return false;
    }

    private static bool HandlePaintTile(GetDataHandlerArgs args)
    {
        short num = args.Data.ReadInt16();
        short num2 = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(num, num2);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权油漆砖!");
        }
        args.Player.SendData(PacketTypes.PaintTile, "", num, num2, Main.tile[num, num2].color());

        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力在被房子保护的地区油漆砖!");
        return true;
    }

    private static bool HandlePaintWall(GetDataHandlerArgs args)
    {
        short num = args.Data.ReadInt16();
        short num2 = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(num, num2);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权油漆墙!");
        }
        args.Player.SendData(PacketTypes.PaintWall, "", num, num2, Main.tile[num, num2].wallColor());

        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }

        args.Player.SendErrorMessage("[i:511]你没有权力在被房子保护的地区油漆墙!");
        return true;
    }

    private static bool HandleTeleport(GetDataHandlerArgs args)
    {
        return false;
    }

    private static bool HandlePlaceObject(GetDataHandlerArgs args)
    {
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改房子保护区域!");
        }
        args.Player.SendTileSquareCentered(x, y);
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护区域!");
        return true;
    }

    private static bool HandlePlaceTileEntity(GetDataHandlerArgs args)
    {
        short x = args.Data.ReadInt16();
        short y = args.Data.ReadInt16();
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改房子保护区域!");
        }
        args.Player.SendTileSquareCentered(x, y);

        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护的地区!");
        return true;
    }

    private static bool HandlePlaceItemFrame(GetDataHandlerArgs args)
    {
        short x = args.Data.ReadInt16();
        short y = args.Data.ReadInt16();
        TEItemFrame tEItemFrame = (TEItemFrame)TileEntity.ByID[TEItemFrame.Find(x, y)];
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改房子保护的物品框!");
        }
        NetMessage.SendData(86, -1, -1, NetworkText.Empty, tEItemFrame.ID, 0f, 1f);
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护的物品框!");
        return true;
    }

    private static bool HandleGemLockToggle(GetDataHandlerArgs args)
    {
        int x = args.Data.ReadInt16();
        int y = args.Data.ReadInt16();
        if (!HousingPlugin.LConfig.保护宝石锁)
        {
            return false;
        }
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权触发房子保护的宝石锁!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力触发被房子保护的宝石锁!");
        return true;
    }

    private static bool HandleMassWireOperation(GetDataHandlerArgs args)
    {
        int num = args.Data.ReadInt16();
        int num2 = args.Data.ReadInt16();
        int num3 = args.Data.ReadInt16();
        int num4 = args.Data.ReadInt16();
        Rectangle value = new Rectangle(Math.Min(num, num3), (args.TPlayer.direction != 1) ? num2 : num4, Math.Abs(num3 - num) + 1, 1);
        Rectangle value2 = new Rectangle((args.TPlayer.direction != 1) ? num3 : num, Math.Min(num2, num4), 1, Math.Abs(num4 - num2) + 1);
        for (int i = 0; i < HousingPlugin.Houses.Count; i++)
        {
            House house = HousingPlugin.Houses[i];
            if (house != null && (house.HouseArea.Intersects(value) || house.HouseArea.Intersects(value2)) && !args.Player.Group.HasPermission(EditHouse) && !(args.Player.Account.ID.ToString() == house.Author) && !HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) && !HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
            {
                return true;
            }
        }
        return false;
    }

    private static bool HandleRequestTileEntityInteraction(GetDataHandlerArgs args)
    {
        int key = args.Data.ReadInt32();
        byte b = args.Data.ReadInt8();
        if (!TileEntity.ByID.TryGetValue(key, out var value))
        {
            return false;
        }
        if (value == null)
        {
            return false;
        }
        House house = HTools.InAreaHouse(value.Position.X, value.Position.Y);
        if (house == null)
        {
            return false;
        }
        if ((!house.Locked || HousingPlugin.LConfig.禁止锁房屋) && !HousingPlugin.LConfig.始终保护箱子)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权打开此模木架!");
        }
        if (args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) <= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力打开被房子保护的地区的模木架!");
        return true;
    }

    private static bool HandleWeaponsRackTryPlacing(GetDataHandlerArgs args)
    {
        short x = args.Data.ReadInt16();
        short y = args.Data.ReadInt16();
        TEWeaponsRack tEWeaponsRack = (TEWeaponsRack)TileEntity.ByID[TEWeaponsRack.Find(x, y)];
        House house = HTools.InAreaHouse(x, y);
        if (house == null)
        {
            return false;
        }
        if (args.Player.Group.HasPermission(EditHouse) || args.Player.Account.ID.ToString() == house.Author || HTools.OwnsHouse(args.Player.Account.ID.ToString(), house) || HTools.CanUseHouse(args.Player.Account.ID.ToString(), house))
        {
            return false;
        }
        if (HousingPlugin.LConfig.冻结警告破坏者)
        {
            args.Player.Disable("[i:511]你无权修改房子保护的武器板!");
        }
        NetMessage.SendData(86, -1, -1, NetworkText.Empty, tEWeaponsRack.ID, 0f, 1f);
        if(args.Player.GetData<DateTime>("HouseWarn") + TimeSpan.FromSeconds(5.0) >= DateTime.Now)
        {
            args.Player.SetData<DateTime>("HouseWarn", DateTime.Now);

        }
        else
        {
            return true;
        }
        args.Player.SendErrorMessage("[i:511]你没有权力修改被房子保护的武器板!");
        return true;
    }
}
