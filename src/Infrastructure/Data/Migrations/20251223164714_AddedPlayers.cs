using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AnswerPlayerId",
                table: "BonusQuestions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AnswerPlayerId",
                table: "BonusPredictions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: true),
                    Position = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PhotoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Age = table.Column<int>(type: "integer", nullable: true),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Height = table.Column<int>(type: "integer", nullable: true),
                    Weight = table.Column<int>(type: "integer", nullable: true),
                    Injured = table.Column<bool>(type: "boolean", nullable: true),
                    ApiFootballId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonusQuestions_AnswerPlayerId",
                table: "BonusQuestions",
                column: "AnswerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusPredictions_AnswerPlayerId",
                table: "BonusPredictions",
                column: "AnswerPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_ApiFootballId",
                table: "Players",
                column: "ApiFootballId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamId",
                table: "Players",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_TeamId_ApiFootballId",
                table: "Players",
                columns: new[] { "TeamId", "ApiFootballId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BonusPredictions_Players_AnswerPlayerId",
                table: "BonusPredictions",
                column: "AnswerPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_BonusQuestions_Players_AnswerPlayerId",
                table: "BonusQuestions",
                column: "AnswerPlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BonusPredictions_Players_AnswerPlayerId",
                table: "BonusPredictions");

            migrationBuilder.DropForeignKey(
                name: "FK_BonusQuestions_Players_AnswerPlayerId",
                table: "BonusQuestions");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropIndex(
                name: "IX_BonusQuestions_AnswerPlayerId",
                table: "BonusQuestions");

            migrationBuilder.DropIndex(
                name: "IX_BonusPredictions_AnswerPlayerId",
                table: "BonusPredictions");

            migrationBuilder.DropColumn(
                name: "AnswerPlayerId",
                table: "BonusQuestions");

            migrationBuilder.DropColumn(
                name: "AnswerPlayerId",
                table: "BonusPredictions");
        }
    }
}
