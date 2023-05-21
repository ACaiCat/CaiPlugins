using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;
using IL.Microsoft.Xna.Framework;
using MySql.Data.MySqlClient;
using TShockAPI;
using TShockAPI.DB;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using Terraria.DataStructures;
using Org.BouncyCastle.Math.Field;
using System.Net;
using System.Collections.Generic;
using IL.Terraria;

namespace Parkour
{

    public class TableManager
    {
        public static List<SqlTable> sqlTables = new List<SqlTable>
        {
            new SqlTable("Parkours",
                                     new SqlColumn("Name", MySqlDbType.String) { Primary = true ,Length = 30  },
                                     new SqlColumn("Region", MySqlDbType.String) { Length = 30 },
                                     new SqlColumn("Record", MySqlDbType.LongText),
                                     new SqlColumn("BagID", MySqlDbType.Int32),
                                     new SqlColumn("Award", MySqlDbType.Int32),
                                     new SqlColumn("AwardCD", MySqlDbType.Int32),
                                     new SqlColumn("AwardCDRecord", MySqlDbType.LongText),
                                     new SqlColumn("SignPos", MySqlDbType.String){ Length = 30 }
    )};

        public static void CreateTables()
        {
            SqlTableCreator sqlTableCreator = new SqlTableCreator(TShock.DB, TShock.DB.GetSqlType() != SqlType.Sqlite ? new MysqlQueryCreator() : new SqliteQueryCreator());
            foreach (SqlTable table in sqlTables)
            {
                sqlTableCreator.EnsureTableStructure(table);
            }
        }


    }
    public static class DB
    {
        public static bool AddParkour(ParkourInfo par)
        {
            try
            {
                TShock.DB.Query(
                    "INSERT INTO Parkours (Name,Region,Record,BagID,Award,AwardCD,AwardCDRecord,SignPos) VALUES (@0,@1,@2,@3,@4,@5,@6,@7);",
                    par.Name, par.Region.Name, JsonSerializer.Serialize(par.Records), par.BagID, par.Award, par.AwardCD, JsonSerializer.Serialize(par.AwardCDRecords), JsonSerializer.Serialize(par.SignPos));
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return false;
            }
        }
        public static bool UpdateParkour(ParkourInfo par)
        {
            try
            {
                TShock.DB.Query(
                    "UPDATE Parkours SET Region=@0,Record=@1,BagID=@2,Award=@3,AwardCD=@4,AwardCDRecord=@5,SignPos=@6 WHERE Name=@7;",
                    par.Region.Name, JsonSerializer.Serialize(par.Records), par.BagID, par.Award, par.AwardCD, JsonSerializer.Serialize(par.AwardCDRecords), JsonSerializer.Serialize(par.SignPos),par.Name);
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return false;
            }
        }


        public static List<ParkourInfo> GetAllParkour()
        {
            List<ParkourInfo> par = new();
            try
            {
                

                using (var reader = TShock.DB.QueryReader("SELECT * FROM Parkours"))
                {
                    while (reader.Read())
                    {
                        par.Add(new(reader.Get<string>("Name"), reader.Get<string>("Region"),
                            reader.Get<int>("BagID"), reader.Get<int>("Award"),
                            reader.Get<int>("AwardCD"), reader.Get<string>("Record"),
                            reader.Get<string>("AwardCDRecord"), reader.Get<string>("SignPos")));
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
            return par;
        }
        public static ParkourInfo GetParkour(string name)
        {
            try
            {
                
                using (var reader = TShock.DB.QueryReader("SELECT * FROM Parkours WHERE Name=@0", name))
                {
                    if (reader.Read())
                    {  
                        return new(reader.Get<string>("Name"),reader.Get<string>("Region"),
                            reader.Get<int>("BagID"),reader.Get<int>("Award"),
                            reader.Get<int>("AwardCD"),reader.Get<string>("Record"), 
                            reader.Get<string>("AwardCDRecord"),reader.Get<string>("SignPos"));
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }
            return new();
        }

        public static bool DeleteParkour(string name)
        {
            try
            {
                TShock.DB.Query(
                    "DELETE FROM Parkours WHERE Name = @0;",
                    name);
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return false;
            }
        }


        public static bool InsertParkour(ParkourInfo parkour)
        {

            

            if (!GetParkour(parkour.Name).exists)
            {
                try
                {
                    AddParkour(parkour);
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(ex.ToString());
                }
            }
            else
            {
                try
                {
                    UpdateParkour(parkour);
                    return true;
                }
                catch (Exception ex)
                {
                    TShock.Log.ConsoleError(ex.ToString());
                }
            }
            return false;
        }
    }

}