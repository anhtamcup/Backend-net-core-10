using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S3.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE pt
                SET CustomerName = latest.CustomerName
                FROM dbo.PaymentTransactions AS pt
                CROSS APPLY
                (
                    SELECT TOP (1) name.CustomerName
                    FROM dbo.PaymentNotifications AS pn
                    CROSS APPLY
                    (
                        VALUES (CASE WHEN ISJSON(pn.Payload) = 1
                            THEN JSON_VALUE(pn.Payload, '$.payload.customerName') END)
                    ) AS name(CustomerName)
                    WHERE pn.PaymentTransactionID = pt.ID
                      AND pn.Provider = 'NAPAS'
                      AND NULLIF(LTRIM(RTRIM(name.CustomerName)), '') IS NOT NULL
                    ORDER BY pn.ReceivedAt DESC, pn.ID DESC
                ) AS latest;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                table: "PaymentTransactions");
        }
    }
}
