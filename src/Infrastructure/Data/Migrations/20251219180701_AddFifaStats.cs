using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFifaStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorldRanking",
                table: "Teams");

            migrationBuilder.AddColumn<decimal>(
                name: "FifaPoints",
                table: "Teams",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FifaRank",
                table: "Teams",
                type: "integer",
                maxLength: 5,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FifaRankingUpdatedAt",
                table: "Teams",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FifaPoints",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "FifaRank",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "FifaRankingUpdatedAt",
                table: "Teams");

            migrationBuilder.AddColumn<int>(
                name: "WorldRanking",
                table: "Teams",
                type: "integer",
                maxLength: 5,
                nullable: false,
                defaultValue: 0);
        }
    }
}
