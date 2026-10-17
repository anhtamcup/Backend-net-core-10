using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DD.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddEkycSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EkycSessions",
                columns: table => new
                {
                    SessionId = table.Column<string>(type: "character varying(36)", maxLength: 36, nullable: false),
                    PartnerId = table.Column<int>(type: "integer", nullable: false),
                    NabPartnerId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CallbackUrl = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    IssuedAt = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResultHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkycSessions", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK_EkycSessions_Partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "Partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EkycSessions_PartnerId_CreatedAt",
                table: "EkycSessions",
                columns: new[] { "PartnerId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EkycSessions");
        }
    }
}
