using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace S3.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class StorePartnerSecretKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SecretKey",
                table: "PartnerCredentials",
                type: "nvarchar(max)",
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
