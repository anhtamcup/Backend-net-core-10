using Dapper;
using Microsoft.Data.Sqlite;
using POS_App.Models;

namespace POS_App.Services
{
    public class ProductService
    {
        private readonly DatabaseService _db;

        public ProductService(DatabaseService db = null)
        {
            if (db == null)
            {
                db = new DatabaseService();
            }

            _db = db;
        }

        private SqliteConnection CreateConnection()
            => new SqliteConnection(_db.ConnectionString);

        public List<YS_Product> GetAll()
        {
            using var conn = CreateConnection();
            return conn.Query<YS_Product>("SELECT * FROM YS_Product").AsList();
        }


        public YS_Product GetByBarcode(string barcode)
        {
            using var conn = CreateConnection();
            return conn.QueryFirstOrDefault<YS_Product>(
                "SELECT * FROM YS_Product WHERE Barcodes = @Barcode LIMIT 1",
                new { Barcode = barcode });
        }

        public List<YS_Product> Search(string keyword)
        {
            using var conn = CreateConnection();
            return conn.Query<YS_Product>(
                "SELECT * FROM YS_Product WHERE ProductName LIKE @kw OR Barcodes LIKE @kw OR ProductCode LIKE @kw",
                new { kw = $"%{keyword}%" }).AsList();
        }

        public YS_Product GetById(int id)
        {
            using var conn = CreateConnection();
            return conn.QueryFirstOrDefault<YS_Product>(
                "SELECT * FROM YS_Product WHERE YS_ProductID = @Id",
                new { Id = id });
        }

        public void Add(YS_Product p)
        {
            using var conn = CreateConnection();
            conn.Execute(@"
                INSERT INTO YS_Product
                    (YS_AccountID, ProductCode, ProductName, ProductDescription,
                     Barcodes, RetailPrice, CreatedDate, CreatedBy, ProductVAT)
                VALUES
                    (@YS_AccountID, @ProductCode, @ProductName, @ProductDescription,
                     @Barcodes, @RetailPrice, @CreatedDate, @CreatedBy, @ProductVAT)", p);
        }

        public void Update(YS_Product p)
        {
            using var conn = CreateConnection();
            conn.Execute(@"
                UPDATE YS_Product SET
                    YS_AccountID       = @YS_AccountID,
                    ProductCode        = @ProductCode,
                    ProductName        = @ProductName,
                    ProductDescription = @ProductDescription,
                    Barcodes           = @Barcodes,
                    RetailPrice        = @RetailPrice,
                    ModifiedDate       = @ModifiedDate,
                    ModifiedBy         = @ModifiedBy,
                    ProductVAT         = @ProductVAT
                WHERE YS_ProductID = @YS_ProductID", p);
        }

        public void Delete(int id)
        {
            using var conn = CreateConnection();
            conn.Execute(
                "DELETE FROM YS_Product WHERE YS_ProductID = @Id",
                new { Id = id });
        }

        public void BulkInsert(List<YS_Product> products)
        {
            using var conn = CreateConnection();

            conn.Open();

            using var tran = conn.BeginTransaction();

            try
            {
                conn.Execute(@"
        INSERT INTO YS_Product
        (
            YS_ProductID,
            YS_AccountID,
            ProductCode,
            ProductName,
            ProductDescription,
            Barcodes,
            RetailPrice,
            CreatedDate,
            CreatedBy,
            ModifiedDate,
            ModifiedBy,
            ProductVAT
        )
        VALUES
        (
            @YS_ProductID,
            @YS_AccountID,
            @ProductCode,
            @ProductName,
            @ProductDescription,
            @Barcodes,
            @RetailPrice,
            @CreatedDate,
            @CreatedBy,
            @ModifiedDate,
            @ModifiedBy,
            @ProductVAT
        )

        ON CONFLICT(YS_ProductID)
        DO UPDATE SET
            YS_AccountID = excluded.YS_AccountID,
            ProductCode = excluded.ProductCode,
            ProductName = excluded.ProductName,
            ProductDescription = excluded.ProductDescription,
            Barcodes = excluded.Barcodes,
            RetailPrice = excluded.RetailPrice,
            ModifiedDate = excluded.ModifiedDate,
            ModifiedBy = excluded.ModifiedBy,
            ProductVAT = excluded.ProductVAT;
        ",
                products,
                tran);

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }
    }
}
