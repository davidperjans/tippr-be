using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PointsEarned",
                table: "Predictions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsScored",
                table: "Predictions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScoredAt",
                table: "Predictions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoredResultVersion",
                table: "Predictions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultVersion",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "Rank",
                table: "LeagueStandings",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsScored",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "ScoredAt",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "ScoredResultVersion",
                table: "Predictions");

            migrationBuilder.DropColumn(
                name: "ResultVersion",
                table: "Matches");

            migrationBuilder.AlterColumn<int>(
                name: "PointsEarned",
                table: "Predictions",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "Rank",
                table: "LeagueStandings",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
