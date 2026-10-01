using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S3.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerCallbackStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PartnerCallbackAt",
                table: "PaymentTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerCallbackCode",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerCallbackError",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerCallbackHttpStatus",
                table: "PaymentTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartnerCallbackMessage",
                table: "PaymentTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerCallbackStatus",
                table: "PaymentTransactions",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTransactions_PartnerCallbackStatus_PartnerCallbackAt",
                table: "PaymentTransactions",
                columns: new[] { "PartnerCallbackStatus", "PartnerCallbackAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PaymentTransactions_PartnerCallbackStatus_PartnerCallbackAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PartnerCallbackAt",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PartnerCallbackCode",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PartnerCallbackError",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PartnerCallbackHttpStatus",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PartnerCallbackMessage",
                table: "PaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PartnerCallbackStatus",
                table: "PaymentTransactions");
        }
    }
}
