using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S3.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class LinkPaymentTransactionRequestLogs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF COL_LENGTH(N'dbo.PaymentTransactions', N'NotificationPaymentID') IS NULL
                    ALTER TABLE dbo.PaymentTransactions ADD NotificationPaymentID nvarchar(max) NULL;

                IF COL_LENGTH(N'dbo.PaymentTransactions', N'RequestCreateQRID') IS NULL
                    ALTER TABLE dbo.PaymentTransactions ADD RequestCreateQRID nvarchar(max) NULL;
                """);

            migrationBuilder.Sql("""
                EXEC(N'CREATE OR ALTER VIEW dbo.PaymentTransactionRequestLogs AS
                SELECT pt.ID AS PaymentTransactionID,
                       CAST(NULL AS int) AS PaymentNotificationID,
                       ''CREATE_QR'' AS RequestType,
                       rl.ID AS RequestLogID,
                       rl.RequestID,
                       rl.CreatedAt AS RequestLogCreatedAt,
                       rl.Path,
                       rl.Method,
                       rl.StatusCode,
                       rl.Duration,
                       rl.RequestHeader,
                       rl.RequestBody,
                       rl.ResponseBody,
                       rl.ErrorMessage,
                       rl.StackTrace
                FROM dbo.PaymentTransactions AS pt
                INNER JOIN dbo.RequestLogs AS rl
                    ON rl.RequestID = pt.RequestCreateQRID
                UNION ALL
                SELECT pn.PaymentTransactionID,
                       pn.ID AS PaymentNotificationID,
                       ''NAPAS_NOTIFICATION'' AS RequestType,
                       rl.ID AS RequestLogID,
                       rl.RequestID,
                       rl.CreatedAt AS RequestLogCreatedAt,
                       rl.Path,
                       rl.Method,
                       rl.StatusCode,
                       rl.Duration,
                       rl.RequestHeader,
                       rl.RequestBody,
                       rl.ResponseBody,
                       rl.ErrorMessage,
                       rl.StackTrace
                FROM dbo.PaymentNotifications AS pn
                INNER JOIN dbo.RequestLogs AS rl
                    ON rl.RequestID = pn.RequestID;');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP VIEW IF EXISTS dbo.PaymentTransactionRequestLogs;

                IF COL_LENGTH(N'dbo.PaymentTransactions', N'NotificationPaymentID') IS NOT NULL
                    ALTER TABLE dbo.PaymentTransactions DROP COLUMN NotificationPaymentID;

                IF COL_LENGTH(N'dbo.PaymentTransactions', N'RequestCreateQRID') IS NOT NULL
                    ALTER TABLE dbo.PaymentTransactions DROP COLUMN RequestCreateQRID;
                """);
        }
    }
}
