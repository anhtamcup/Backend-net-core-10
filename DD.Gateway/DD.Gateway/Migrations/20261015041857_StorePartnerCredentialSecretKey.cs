using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DD.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class StorePartnerCredentialSecretKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "PartnerCredentials",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SecretKey",
                table: "PartnerCredentials");
        }
    }
}
