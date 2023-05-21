using System.Text.Json;
using Microsoft.Xna.Framework;
using SSCManager;
using Terraria.DataStructures;
using TShockAPI;
using TShockAPI.DB;

namespace Parkour;


public class ParkourPlay
{
    public TSPlayer player { get; set; }
    public ParkourInfo parkour { get; set; }

    public ParkourInfo.Point16 StartPoint { get; set; }

    public DateTime startTime { get; set; }

    public DateTime endTime { get; set; }

    public Vector2 SpawnPoint;

    public int DeathTimes = 0;

    public DateTime lastDeathTime = DateTime.MinValue;
    public bool CanSetSpawn
    {
        get
        {
            return DateTime.Now - lastDeathTime >= TimeSpan.FromSeconds(5);
        }
    }
    public TimeSpan currentTime
    {
        get
        {
            return DateTime.Now - startTime;
        }
    }
    public TimeSpan totalTime
    {
        get
        {
            return endTime - startTime;
        }
    }

    public string GetTime
    {
        get
        {
            return Math.Round(currentTime.TotalSeconds, 2).ToString();
        }
    }
    public string GetFinalTime
    {
        get
        {
            return Math.Round(totalTime.TotalSeconds, 2).ToString();
        }
    }
    public bool IsPlaying { get; set; } = false;

    public void Start()
    {
        IsPlaying = true;
        startTime = DateTime.Now;
        SpawnPoint = player.TPlayer.position;
        SSCSaver.RestoryBag(player, parkour.BagID, false);
    }

    public void End()
    {
        IsPlaying = false;
        endTime = DateTime.Now;
    }


    public ParkourPlay(TSPlayer player, ParkourInfo parkour)
    {
        this.player = player;
        this.StartPoint = new(player.TileX, player.TileY);
        this.parkour = parkour;
     
        Start();
    }
}

public class ParkourInfo
{
   public  class Point16
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Point16(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return $"X:{X},Y:{Y}";
        }

    }
    public string Name { get; set; } = null!;
    public Region RegionDate { get; set; }
    public Region Region
    {
        get
        {
            if (RegionDate == null)
            {
                return TShock.Regions.GetRegionByName(RegionName);
            }
            else
            {
                return RegionDate;
            }
        }
      
    }

    public string RegionName { get; set; } = null!;
    public int BagID { get; set; } = 0;

    public int Award = 0;

    public int AwardCD = 0;

    public bool exists = false;

    public Point16 SignPos { get; set; } = new(-1, -1);

    public SelectPoint select = new();

    public Dictionary<int, TimeSpan> Records { get; set; }

    public KeyValuePair<int, TimeSpan> FastestRecord
    {
        get
        {
            //找到Records里TimeSpan最小的页
            var min = Records.Aggregate((l, r) => l.Value < r.Value ? l : r);
            return min;
        }
    }

    public Dictionary<int, DateTime> AwardCDRecords { get; set; }

    public string GetRecord(int index)
    {
        return Math.Round(Records[index].TotalSeconds,2).ToString();
    }
    public ParkourInfo()
    {
        exists = false;
    }

    public enum SelectPoint
    {
        Start,
        TempAdd,
        TempRemove,
        End,
        None
    }

    public ParkourInfo(string name, string region, int bagID, int award, int awardCD, string records, string awardCDRecords,string sign)
    {
        Name = name;
        RegionName = region;
        BagID = bagID;
        Award = award;
        AwardCD = awardCD;
        Records = JsonSerializer.Deserialize<Dictionary<int, TimeSpan>>(records)!;
        AwardCDRecords = JsonSerializer.Deserialize<Dictionary<int, DateTime>>(awardCDRecords)!;
        SignPos = JsonSerializer.Deserialize<Point16>(sign);
        exists = true;

    }
    public ParkourInfo(string name, string region, int bagID, int award, int awardCD)
    {
        Name = name;
        RegionName = region;
        BagID = bagID;
        Award = award;
        AwardCD = awardCD;
        Records = new();
        SignPos = new(-1,-1);
        AwardCDRecords = new();
        exists = true;

    }


    public static ParkourInfo AddParkour(string name, string region, int bagID, int award, int awardCD)
    {
        var parkour = new ParkourInfo(name, region, bagID, award, awardCD);
        DB.InsertParkour(parkour);
        return parkour;
    }

    public void SaveParkour()
    {
        DB.InsertParkour(this);
    }

}
public static class Utils
{
    public static ParkourPlay GetParkourByName(this List<ParkourPlay> parkourPlays, string name)
    {
        return parkourPlays.Find(x => x.player.Name == name);
    }


    public static void RewardPlayer(this TSPlayer plr,int rewards)
    {
        Task.Run(delegate
        {
            try 
            {
                XSB.Utils.GetData($"GiveGold?name={plr.Name}&gold={rewards}", true); 
            } 
            catch(Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString()); 
            }
        });
    }
}