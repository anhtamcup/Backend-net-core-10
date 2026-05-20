using Dapper;
using Microsoft.Data.Sqlite;
using System.IO;

namespace POS_App.Services
{
    public class DatabaseService
    {
        public readonly string ConnectionString;

        public DatabaseService()
        {
            var dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.db");
            ConnectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private SqliteConnection CreateConnection()
            => new SqliteConnection(ConnectionString);

        private void InitializeDatabase()
        {
            using var conn = CreateConnection();
            conn.Execute(@"
                CREATE TABLE IF NOT EXISTS YS_Product (
                    YS_ProductID       INTEGER PRIMARY KEY AUTOINCREMENT,
                    YS_AccountID       INTEGER,
                    ProductCode        TEXT,
                    ProductName        TEXT,
                    ProductDescription TEXT,
                    Barcodes           TEXT,
                    RetailPrice        REAL,
                    CreatedDate        TEXT,
                    CreatedBy          INTEGER,
                    ModifiedDate       TEXT,
                    ModifiedBy         INTEGER,
                    ProductVAT         TEXT
                );
            ");
        }
    }
}