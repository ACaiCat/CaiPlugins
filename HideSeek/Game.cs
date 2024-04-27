using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Terraria;
using Steamworks;
using TShockAPI;
using Terraria.DataStructures;
using SSCManager;
using Org.BouncyCastle.Asn1.X509;
using Terraria.ID;
using XSB;
using System.Data;

namespace HideSeek
{
    internal class Game
    {
        public List<Player> Players = new();
        public List<Player> CatPlayers
        {
            get
            {
                return Players.Where(i => i.Role == Role.Cat).ToList();
            }
        }

        public List<Player> MousePlayers
        {
            get
            {
                return Players.Where(i => i.Role == Role.Mouse).ToList();
            }
        }

        public DateTime GameStartTime;

        public bool isGameing = false;
        public bool isWaitEnd = false;
        public int GameLastSecond
        {
            get
            {
                return (int)(DateTime.Now - GameStartTime).TotalSeconds;
            }
        }

        public bool isGameTimeOut
        {
            get
            {
                return GameLastSecond >= Config.config.GameLast;
            }
        }

        public bool isGameCatDead
        {
            get
            {
                return Players.Count(p => p.Role == Role.Cat && !p.isDead) == 0;
            }
        }

        public bool isGameMouseDead
        {
            get
            {
                return Players.Count(p => p.Role == Role.Mouse && !p.isDead) == 0;
            }
        }

        public void GameSpawnItem()
        {
            foreach (var item in Config.config.ItemSpawn)
            {
                int num = Item.NewItem(new EntitySource_DebugCommand(), item.Key.X, item.Key.Y, item.Value, 1, 1);
                TSPlayer.All.SendData(PacketTypes.UpdateItemDrop, null, num);
                TSPlayer.All.SendData(PacketTypes.ItemOwner, null, num);
                TSPlayer.All.SendData(PacketTypes.TweakItem, null, num, 255f, 63f);
            }
        }

        public void Broadcast(string text)
        {
            foreach (var i in Players)
            {
                if (i.TSPlayer == null)
                {
                    continue;
                }
                i.TSPlayer.SendInfoMessage(text);
            }
        }

        public void CatBroadcast(string text)
        {
            foreach (var i in CatPlayers)
            {
                if (i.TSPlayer == null)
                {
                    continue;
                }
                i.TSPlayer.SendInfoMessage(text);
            }
        }

        public void MouseBroadcast(string text)
        {
            foreach (var i in MousePlayers)
            {
                if (i.TSPlayer == null)
                {
                    continue;
                }
                i.TSPlayer.SendInfoMessage(text);
            }
        }

        public void Join(TSPlayer player)
        {
            if (!isGameing)
            {
                Players.Add(new Player(player));
                player.RestoryBag(Config.config.JoinSSC);
                player.Teleport(Config.config.GameRoom.X, Config.config.GameRoom.Y);
                player.SendInfoMessage("[i:1991]欢迎加入躲猫猫游戏 (By 坚果、Cai等)");
                player.SendInfoMessage("[i:1810]手持[c/c35a15:霉运砂球]选择[c/c35a15:猫猫]");
                player.SendInfoMessage("[i:2163]手持[c/5a5ae2:松鼠笼子]选择[c/5a5ae2:鼠鼠]");
            }
            else
            {
                player.SendInfoMessage("[i:1299]游戏已开始,已为你开启观战模式");
                player.RestoryBag(Config.config.JoinSSC);
                Players.Add(new Player(player) { isDead = true });
                player.TPlayer.ghost = true;
                player.SendData(PacketTypes.PlayerUpdate, null, player.Index);
                player.Teleport(Config.config.MouseStart.X, Config.config.MouseStart.Y);
            }

        }
        private static void ShuffleList<T>(IList<T> list)
        {
            Random random = new Random();

            for (int i = list.Count - 1; i > 0; --i)
            {
                int j = random.Next(i + 1);

                T temp = list[j];
                list[j] = list[i];
                list[i] = temp;
            }
        }
        public void GiveRole()
        {
            foreach (var i in Players)
            {
                if (i.TSPlayer.SelectedItem.netID == 1810)
                {
                    i.WantRole = Role.Cat;
                }
                if (i.TSPlayer.SelectedItem.netID == 2163)
                {
                    i.WantRole = Role.Mouse;
                }
            }
            List<Player> Cats = new();
            List<Player> Mouses = new();
            foreach (var i in Players)
            {
                if (i.WantRole == Role.Cat)
                {
                    Cats.Add(i);
                }
                else
                {
                    Mouses.Add(i);
                }
            }
            ShuffleList(Cats);
            ShuffleList(Mouses);
            int catCount = Players.Count / 5;
            if (catCount == 0)
            {
                catCount = 1;
            }
            if (Cats.Count > catCount)
            {
                for (int i = 0; i < Cats.Count- catCount; i++)
                {
                    Mouses.Add(Cats[0]);
                    Cats.RemoveAt(0);
                }
            }
            if (Cats.Count < catCount)
            {
                for (int i = 0; i < catCount - Cats.Count; i++)
                {
                    Cats.Add(Mouses[0]);
                    Mouses.RemoveAt(0);
                }
            }
            foreach (var i in Cats)
            {
                i.Role = Role.Cat;
            }
            foreach (var i in Mouses)
            {
                i.Role = Role.Mouse;
            }

        }

        public void GameStart()
        {

            Broadcast("[i:4095]开始分配角色...");
            GiveRole();
            GameStartTime = DateTime.Now;
            isGameing = true;
            foreach (var i in Players)
            {
                Task.Run(()=>
                {
                    if (i.TSPlayer == null)
                    {
                        return;
                    }
                    if (i.Role == Role.Cat)
                    {

                        i.TSPlayer.RestoryBag(Config.config.CatSSC);
                        Thread.Sleep(1000);
                        if (i.TSPlayer == null)
                        {
                            return;
                        }
                        i.TSPlayer.Teleport(Config.config.CatStart.X, Config.config.CatStart.Y);
                        i.TSPlayer.SetPvP(true);
                        i.TSPlayer.SetTeam(3);
                    }
                    else
                    {
                        i.TSPlayer.RestoryBag(Config.config.MouseSSC);
                        Thread.Sleep(2000);
                        if (i.TSPlayer == null)
                        {
                            return;
                        }
                        i.TSPlayer.Teleport(Config.config.MouseStart.X, Config.config.MouseStart.Y);
                        i.TSPlayer.SetPvP(true);
                        i.TSPlayer.SetTeam(4);
                    }
                });

            }
            CatBroadcast("[i:3183]你是[c/FB394B:猫猫],你的目标是在倒计时结束前发现、杀光所有鼠鼠，并且活到最后!");
            MouseBroadcast("[i:3183]你是[c/FB394B:鼠鼠],你的目标是活到倒计时结束，或者杀光所有猫猫!");

        }

        public void GameEnd()
        {


            if (isGameMouseDead)
            {

                Broadcast("[i:2608]游戏结束,猫猫杀光了所有鼠鼠，[c/FB394B:猫猫]获胜!");
                CatBroadcast($"[i:855]本局获得[c/FB394B:{Config.config.WinMoney}金币]奖励,请注意查收哦！");
                MouseBroadcast($"[i:855]本局获得[c/FB394B:{Config.config.LoseMoney}金币]奖励,请注意查收哦！");
                foreach (var i in Players)
                {
                    if (i.TSPlayer == null)
                    {
                        continue;
                    }
                    if (i.Role==Role.Cat)
                    {
                        int p = Projectile.NewProjectile(Projectile.GetNoneSource(), i.TSPlayer.TPlayer.position.X, i.TSPlayer.TPlayer.position.Y - 64f, 0f, -8f, ProjectileID.RocketFireworksBoxRed, 0, 0);
                        Main.projectile[p].Kill();
                        i.TSPlayer.RewardPlayer(Config.config.WinMoney);
                    }
                    if (i.Role == Role.Mouse)
                    {
                        i.TSPlayer.RewardPlayer(Config.config.LoseMoney);
                    }
                    
                }

            }
            else
            {
                if (isGameCatDead)
                {
                    Broadcast("[i:2608]游戏结束，鼠鼠杀光了所有猫猫，[c/FB394B:鼠鼠]获胜!");
                }
                if (isGameTimeOut)
                {
                    Broadcast("[i:3099]时间到!仍有鼠鼠存活，[c/FB394B:鼠鼠]获胜!");
                }
                CatBroadcast($"[i:855]本局获得[c/FB394B:{Config.config.LoseMoney}金币]奖励,请注意查收哦！");
                MouseBroadcast($"[i:855]本局获得[c/FB394B:{Config.config.WinMoney}金币]奖励,请注意查收哦！");
                foreach (var i in Players)
                {
                    if (i.TSPlayer == null)
                    {
                        continue;
                    }
                    if (i.Role == Role.Mouse)
                    {
                        int p = Projectile.NewProjectile(Projectile.GetNoneSource(), i.TSPlayer.TPlayer.position.X, i.TSPlayer.TPlayer.position.Y - 64f, 0f, -8f, ProjectileID.RocketFireworksBoxRed, 0, 0);
                        Main.projectile[p].Kill();
                        i.TSPlayer.RewardPlayer(Config.config.WinMoney);
                    }
                    if (i.Role == Role.Cat) 
                    {
                        i.TSPlayer.RewardPlayer(Config.config.LoseMoney);
                    }
                    
                }

            }
            for (int i = 0; i < Main.maxItems; i++)
            {
                if (Main.item[i].active && Config.config.Region.InArea((int)Main.item[i].position.X/16, (int)Main.item[i].position.Y / 16))
                {
                    Main.item[i].active = false;
                    TSPlayer.All.SendData(PacketTypes.ItemDrop, "", i);
                }
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                if (Main.projectile[i].active &&  Config.config.Region.InArea((int)Main.projectile[i].position.X / 16, (int)Main.projectile[i].position.Y / 16))
                {
                    Main.projectile[i].active = false;
                    Main.projectile[i].type = 0;
                    TSPlayer.All.SendData(PacketTypes.ProjectileNew, "", i);
                }
            }
            isWaitEnd = true;
            Task.Run(() =>
            {
                Thread.Sleep(10000);

                foreach (var i in Players)
                {
                    i.TSPlayer.SetPvP(false);
                    i.TSPlayer.SetTeam(2);
                    i.TSPlayer.Teleport(Config.config.GameRoom.X, Config.config.GameRoom.Y);
                    i.TSPlayer.RestoryBag(Config.config.JoinSSC);
                    i.TSPlayer.TPlayer.ghost = false;
                    i.TSPlayer.SendData(PacketTypes.PlayerUpdate, null, i.TSPlayer.Index);
                    
                }
                isWaitEnd = false;
                isGameing = false;
                Players.Clear(); 
            });


        }
    }


}
