using Microsoft.EntityFrameworkCore.Migrations;

namespace SecurityToy.Migrations
{
    public partial class TokenPurpose : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenPurpose",
                table: "VerificationTokens",
                nullable: true,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenPurpose",
                table: "VerificationTokens");
        }
    }
}
