using System.Data;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;
using TShockAPI;
using TShockAPI.DB;

namespace SwitchCommands
{
    public class Database
    {
        public Dictionary<string, CommandInfo> switchCommandList = new Dictionary<string, CommandInfo>();

    }
        public class TableManager
    {
        public static List<SqlTable> sqlTables = new List<SqlTable>
        {
            new SqlTable("SwitchCommands",
                                     new SqlColumn("Point", MySqlDbType.String){ Primary = true },
                                     new SqlColumn("Commands", MySqlDbType.String),
                                     new SqlColumn("ignorePerms", MySqlDbType.Int32),
                                     new SqlColumn("cooldown", MySqlDbType.Int32)
                                     
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

        public static Database LoadAll()
        {
            var result = new Database();
            try
            {
                
                using (var reader = TShock.DB.QueryReader("SELECT * FROM SwitchCommands"))
                {
                    while (reader.Read())
                    {
                        var point = new SwitchPos(reader.Get<string>("Point"));
                        result.switchCommandList.Add(point.ToString(), new CommandInfo()
                        {
                            ignorePerms = reader.Get<int>("ignorePerms") == 0 ? false : true,
                            cooldown = reader.Get<int>("cooldown"),
                            point = point,
                            commandList = reader.Get<string>("Commands").Split(',').ToList()

                        });
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }

            return result;
        }

        public static void SaveAll(Database database)
        {
            foreach (var i in database.switchCommandList)
            {
                DB.InsertPoint(i.Value,i.Value.point);
            }
        }

        public static CommandInfo GetPoint(SwitchPos pos)
        {
            try
            {
                using (var reader = TShock.DB.QueryReader("SELECT * FROM SwitchCommands WHERE Point=@0", pos.ToSqlString()))
                {
                    if (reader.Read())
                    {
                        return new()
                        {
                            ignorePerms = reader.Get<int>("ignorePerms") == 0 ? false : true,
                            cooldown = reader.Get<int>("cooldown"),
                            commandList = reader.Get<string>("Commands").Split(',').ToList(),
                            point  = new SwitchPos(reader.Get<string>("Point")),

                        };
                    }
                }
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
            }

            return null;
        }


        public static bool DeletePoint(SwitchPos pos)
        {
            try
            {
                TShock.DB.Query(
                    "DELETE FROM SwitchCommands WHERE Point = @0;",
                    pos.ToSqlString());
                return true;
            }
            catch (Exception ex)
            {
                TShock.Log.ConsoleError(ex.ToString());
                return false;
            }
        }


        public static bool InsertPoint(CommandInfo info, SwitchPos pos)
        {

            if (GetPoint(info.point)==null)
            {
                try
                {
                    TShock.DB.Query(
                        "INSERT INTO SwitchCommands (Point, Commands, ignorePerms, cooldown) VALUES (@0, @1, @2, @3);",
                        info.point.ToSqlString(),string.Join(',',info.commandList),info.ignorePerms?1:0,info.cooldown
                        );
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
                    TShock.DB.Query(
                        "UPDATE SwitchCommands SET Commands = @1, ignorePerms=@2, cooldown=@3 WHERE Point = @0;",
                    pos.ToSqlString(), string.Join(',', info.commandList), info.ignorePerms ? 1 : 0, info.cooldown
                        ); return true;
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