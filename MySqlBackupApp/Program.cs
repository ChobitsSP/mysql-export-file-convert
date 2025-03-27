using MySqlConnector;
using System.IO.Compression;
using Dapper;
using System.Data;

namespace MySqlBackupApp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            LogService.Init();

            try
            {
                var connectionString = ConfigUtils.GetConnectionString("Default");
                connectionString = GetConnectionString(connectionString, (args != null && args.Length > 0) ? args[0] : null);
                var savePath = ConfigUtils.GetSectionValue("DbBackup:SavePath");
                StartBackup(connectionString, savePath);
            }
            catch (Exception ex)
            {
                LogService.Error(ex);
            }
        }

        static string GetConnectionString(string connectionString, string dbName = null)
        {
            if (string.IsNullOrEmpty(dbName)) return connectionString;
            var builder = new MySqlConnectionStringBuilder(connectionString);
            builder.Database = dbName;
            return builder.ConnectionString;
        }

        static void StartBackup(string connectionString, string savePath)
        {
            using var conn = new MySqlConnection(connectionString);

            var dbName = conn.Database;
            var saveName = $"{dbName}_{DateTime.Now.ToString("yyyyMMddHHmmss")}";
            var dirPath = Path.Combine(savePath, saveName);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            var names = GetTableNames(conn);

            using var cmd = conn.CreateCommand();
            using var mb = new MySqlBackup(cmd);

            conn.Open();

            foreach (var name in names)
            {
                try
                {
                    ExportSingleTable(mb, name, dirPath);
                }
                catch (Exception ex)
                {
                    LogService.Warn($"Export table {name} failed.");
                    LogService.Error(ex);
                }
            }

            conn.Close();

            string zipPath = Path.Combine(savePath, saveName + ".zip");
            ZipFile.CreateFromDirectory(dirPath, zipPath);
            Directory.Delete(dirPath, true);
        }

        static List<string> GetTableNames(IDbConnection db)
        {
            var sql = $"SELECT table_name FROM information_schema.tables WHERE table_schema = '{db.Database}'";
            return db.Query<string>(sql).AsList();
        }

        static void ExportSingleTable(MySqlBackup mb, string tableName, string dirPath)
        {
            LogService.Info($"Exporting table {tableName}...");
            var dic = new Dictionary<string, string>();
            dic[tableName] = $"SELECT * FROM `{tableName}`";
            mb.ExportInfo.TablesToBeExportedDic = dic;
            mb.ExportToFile(Path.Combine(dirPath, tableName + ".sql"));
            LogService.Info($"Table {tableName} exported.");
        }
    }
}
