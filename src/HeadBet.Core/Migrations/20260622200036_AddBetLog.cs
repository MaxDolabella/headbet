using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeadBet.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddBetLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BetLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MatchId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PoolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<int>(type: "INTEGER", nullable: false),
                    OldHomeScore = table.Column<int>(type: "INTEGER", nullable: true),
                    OldAwayScore = table.Column<int>(type: "INTEGER", nullable: true),
                    NewHomeScore = table.Column<int>(type: "INTEGER", nullable: false),
                    NewAwayScore = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BetLog", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BetLog_MatchId_UserId",
                table: "BetLog",
                columns: new[] { "MatchId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_BetLog_PoolId",
                table: "BetLog",
                column: "PoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BetLog");
        }
    }
}
