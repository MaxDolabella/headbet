using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HeadBet.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PoolId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Scope = table.Column<int>(type: "INTEGER", nullable: false),
                    MatchId = table.Column<Guid>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessage_Match_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Match",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChatMessage_Pool_PoolId",
                        column: x => x.PoolId,
                        principalTable: "Pool",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessage_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_MatchId",
                table: "ChatMessage",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_PoolId_Scope_MatchId_CreatedAt",
                table: "ChatMessage",
                columns: new[] { "PoolId", "Scope", "MatchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessage_UserId",
                table: "ChatMessage",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatMessage");
        }
    }
}
