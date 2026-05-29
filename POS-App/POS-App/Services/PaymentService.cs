using Dapper;
using Microsoft.Data.Sqlite;
using POS_App.Models;

namespace POS_App.Services
{
    public class PaymentService
    {
        private readonly DatabaseService _db;

        public PaymentService(DatabaseService db = null)
        {
            if (db == null)
            {
                db = new DatabaseService();
            }

            _db = db;
        }

        private SqliteConnection CreateConnection()
            => new SqliteConnection(_db.ConnectionString);

        public List<YS_Payment> GetAll()
        {
            using var conn = CreateConnection();
            return conn.Query<YS_Payment>("SELECT * FROM YS_Payment").AsList();
        }

        public void BulkInsert(List<YS_Payment> payments)
        {
            using var conn = CreateConnection();
            conn.Open();
            using var tran = conn.BeginTransaction();
            try
            {
                conn.Execute(@"
                    INSERT INTO YS_Payment
                    (
                        PaymentID,
                        PaymentName,
                        PaymentValue,
                        GroupMethodValue,
                        GroupMethodName
                    )
                    VALUES
                    (
                        @PaymentID,
                        @PaymentName,
                        @PaymentValue,
                        @GroupMethodValue,
                        @GroupMethodName
                    )

                    ON CONFLICT(PaymentID)
                    DO UPDATE SET
                        PaymentName = excluded.PaymentName,
                        PaymentValue = excluded.PaymentValue,
                        GroupMethodValue = excluded.GroupMethodValue,
                        GroupMethodName = excluded.GroupMethodName;
                    ",
                payments,
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
