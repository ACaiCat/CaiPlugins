using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using Steamworks;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;

namespace StatusMananger
{
    public class DBMananger
    {
        public static IDbConnection DB  => TShock.DB;
        public static List<SqlTable>  sqlTables = new List<SqlTable>
        {
            new SqlTable("StatusManager",
                                     new SqlColumn("UserID", MySqlDbType.Int32) { Primary = true },
                                     new SqlColumn("EnableStatus", MySqlDbType.Int32)
        )};
        

        public static void CreateTables()
        {
            SqlTableCreator sqlTableCreator = new SqlTableCreator(DB, (DB.GetSqlType() != SqlType.Sqlite?new MysqlQueryCreator():new SqliteQueryCreator()));
            foreach (SqlTable table in sqlTables)
            {
                sqlTableCreator.EnsureTableStructure(table);
            }
        }

        public static void AddUser(int UserID)
        {
            using (var reader = DB.QueryReader("SELECT * FROM StatusManager WHERE UserID = @0", UserID))
            {
                if (!reader.Read())
                {
                    DB.Query("INSERT INTO StatusManager (UserID, EnableStatus) VALUES (@0, @1)", UserID, 1);
                }
            }
        }
        public static void UpdateUser(int UserID, bool EnableStatus)
        {
            DB.Query("UPDATE StatusManager SET EnableStatus = @0 WHERE UserID = @1", EnableStatus?1:0, UserID);
        }
        
    }
}