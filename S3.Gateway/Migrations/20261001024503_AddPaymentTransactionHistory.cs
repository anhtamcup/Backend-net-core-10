using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S3.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentTransactionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PaymentTransactions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerID = table.Column<int>(type: "int", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClientRequestID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    GatewayRequestID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderTransactionID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProviderReferenceID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallbackRoutingID = table.Column<int>(type: "int", nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BankID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BillNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QRType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ProviderStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastNotificationAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RawCreateRequest = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTransactions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PaymentTransactions_Partners_PartnerID",
                        column: x => x.PartnerID,
                        principalTable: "Partners",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentNotifications",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentTransactionID = table.Column<int>(type: "int", nullable: false),
                    RequestID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Provider = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProviderTransactionID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ProviderReferenceID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProviderStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessingStatus = table.Column<int>(type: "int", nullable: false),
                    IsDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentNotifications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PaymentNotifications_PaymentTransactions_PaymentTransactionID",
                        column: x => x.PaymentTransactionID,
                        principalTable: "PaymentTransactions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_PaymentTransactionID",
                table: "PaymentNotifications",
                column: "PaymentTransactionID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_Provider_ProviderTransactionID",
                table: "PaymentNotifications",
                columns: new[] { "Provider", "ProviderTransactionID" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentNotifications_RequestID",
                table: "PaymentNotifications",
                column: "RequestID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_ClientRequestID",
                table: "PaymentTransactions",
                column: "ClientRequestID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PartnerID_CreatedAt",
                table: "PaymentTransactions",
                columns: new[] { "PartnerID", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_Provider_ProviderTransactionID",
                table: "PaymentTransactions",
                columns: new[] { "Provider", "ProviderTransactionID" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentNotifications");

            migrationBuilder.DropTable(
                name: "PaymentTransactions");
        }
    }
}
