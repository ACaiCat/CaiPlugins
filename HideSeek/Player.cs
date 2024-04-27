using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI;

namespace HideSeek
{
    internal class Player
    {
        public Player(TSPlayer tsPlayer)
        {
            TSPlayer = tsPlayer;
            AccountName = tsPlayer.Account.Name;
            Role = Role.Waiter;
            WantRole = Role.Waiter;
        }

        public TSPlayer TSPlayer;
        public Role Role;
        public Role WantRole;
        public string AccountName;
        public bool isGameing
        {
            get
            {
                return TShock.Players.Any(i=>i.Account.Name==AccountName);
            }
        } 
        public bool isDead = false;

        
    }
    
    public enum Role
    {
        Cat,
        Mouse,
        Waiter
    }
}
