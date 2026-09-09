using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S3.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerAccessKeyAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestHeader",
                table: "RequestLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientRequestID",
                table: "CallbackRoutings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerCredentialID",
                table: "CallbackRoutings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PartnerID",
                table: "CallbackRoutings",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RefID",
                table: "CallbackRoutingLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestHeader",
                table: "ApiLogs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Partners",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partners", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PartnerCredentials",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PartnerID = table.Column<int>(type: "int", nullable: false),
                    AccessKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SecretKeyHash = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Scopes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RevokedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartnerCredentials", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PartnerCredentials_Partners_PartnerID",
                        column: x => x.PartnerID,
                        principalTable: "Partners",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallbackRoutings_PartnerCredentialID",
                table: "CallbackRoutings",
                column: "PartnerCredentialID");

            migrationBuilder.CreateIndex(
                name: "IX_CallbackRoutings_PartnerID_Target_ClientRequestID",
                table: "CallbackRoutings",
                columns: new[] { "PartnerID", "Target", "ClientRequestID" },
                unique: true,
                filter: "[PartnerID] IS NOT NULL AND [ClientRequestID] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PartnerCredentials_AccessKey",
                table: "PartnerCredentials",
                column: "AccessKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartnerCredentials_PartnerID",
                table: "PartnerCredentials",
                column: "PartnerID");

            migrationBuilder.CreateIndex(
                name: "IX_Partners_Code",
                table: "Partners",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CallbackRoutings_PartnerCredentials_PartnerCredentialID",
                table: "CallbackRoutings",
                column: "PartnerCredentialID",
                principalTable: "PartnerCredentials",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CallbackRoutings_Partners_PartnerID",
                table: "CallbackRoutings",
                column: "PartnerID",
                principalTable: "Partners",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CallbackRoutings_PartnerCredentials_PartnerCredentialID",
                table: "CallbackRoutings");

            migrationBuilder.DropForeignKey(
                name: "FK_CallbackRoutings_Partners_PartnerID",
                table: "CallbackRoutings");

            migrationBuilder.DropTable(
                name: "PartnerCredentials");

            migrationBuilder.DropTable(
                name: "Partners");

            migrationBuilder.DropIndex(
                name: "IX_CallbackRoutings_PartnerCredentialID",
                table: "CallbackRoutings");

            migrationBuilder.DropIndex(
                name: "IX_CallbackRoutings_PartnerID_Target_ClientRequestID",
                table: "CallbackRoutings");

            migrationBuilder.DropColumn(
                name: "RequestHeader",
                table: "RequestLogs");

            migrationBuilder.DropColumn(
                name: "ClientRequestID",
                table: "CallbackRoutings");

            migrationBuilder.DropColumn(
                name: "PartnerCredentialID",
                table: "CallbackRoutings");

            migrationBuilder.DropColumn(
                name: "PartnerID",
                table: "CallbackRoutings");

            migrationBuilder.DropColumn(
                name: "RefID",
                table: "CallbackRoutingLogs");

            migrationBuilder.DropColumn(
                name: "RequestHeader",
                table: "ApiLogs");
        }
    }
}
