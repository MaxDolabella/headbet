using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeadBet.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchBroadcastUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BroadcastUrl",
                table: "Match",
                type: "TEXT",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BroadcastUrl",
                table: "Match");
        }
    }
}
