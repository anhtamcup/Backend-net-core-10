using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DD.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddEkycSessionProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NabPartnerId",
                table: "EkycSessions",
                newName: "ProviderPartnerId");

            migrationBuilder.AddColumn<string>(
                name: "Provider",
                table: "EkycSessions",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "NAMABANK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Provider",
                table: "EkycSessions");

            migrationBuilder.RenameColumn(
                name: "ProviderPartnerId",
                table: "EkycSessions",
                newName: "NabPartnerId");
        }
    }
}
