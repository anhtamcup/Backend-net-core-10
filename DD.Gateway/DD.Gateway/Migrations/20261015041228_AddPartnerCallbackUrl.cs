using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DD.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerCallbackUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CallbackUrl",
                table: "Partners",
                type: "character varying(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CallbackUrl",
                table: "Partners");
        }
    }
}
