using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddedFootballApi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FlagUrl",
                table: "Teams",
                newName: "LogoUrl");

            migrationBuilder.RenameColumn(
                name: "Venue",
                table: "Matches",
                newName: "VenueName");

            migrationBuilder.AlterColumn<string>(
                name: "Role",
                table: "Users",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBanned",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<bool>(
                name: "ApiFootballEnabled",
                table: "Tournaments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ApiFootballLeagueId",
                table: "Tournaments",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApiFootballSeason",
                table: "Tournaments",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GroupName",
                table: "Teams",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Teams",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3);

            migrationBuilder.AddColumn<int>(
                name: "FoundedYear",
                table: "Teams",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Teams",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PointsEarned",
                table: "Predictions",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<bool>(
                name: "IsScored",
                table: "Predictions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "ResultVersion",
                table: "Matches",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "VenueCity",
                table: "Matches",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VenueId",
                table: "Matches",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ExternalSyncStates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "ApiFootball"),
                    Resource = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastSyncedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NextAllowedSyncAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalSyncStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExternalSyncStates_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchEventsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchEventsSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchEventsSnapshots_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchHeadToHeadSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchHeadToHeadSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchHeadToHeadSnapshots_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchLineupSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchLineupSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchLineupSnapshots_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatchStatsSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Json = table.Column<string>(type: "text", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchStatsSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchStatsSnapshots_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Venues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ApiFootballId = table.Column<int>(type: "integer", nullable: true),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "integer", nullable: true),
                    Surface = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Venues", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Teams_VenueId",
                table: "Teams",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_VenueId",
                table: "Matches",
                column: "VenueId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSyncStates_TournamentId",
                table: "ExternalSyncStates",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalSyncStates_TournamentId_Provider_Resource",
                table: "ExternalSyncStates",
                columns: new[] { "TournamentId", "Provider", "Resource" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchEventsSnapshots_FetchedAt",
                table: "MatchEventsSnapshots",
                column: "FetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchEventsSnapshots_MatchId",
                table: "MatchEventsSnapshots",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHeadToHeadSnapshots_FetchedAt",
                table: "MatchHeadToHeadSnapshots",
                column: "FetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchHeadToHeadSnapshots_MatchId",
                table: "MatchHeadToHeadSnapshots",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineupSnapshots_FetchedAt",
                table: "MatchLineupSnapshots",
                column: "FetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchLineupSnapshots_MatchId",
                table: "MatchLineupSnapshots",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchStatsSnapshots_FetchedAt",
                table: "MatchStatsSnapshots",
                column: "FetchedAt");

            migrationBuilder.CreateIndex(
                name: "IX_MatchStatsSnapshots_MatchId",
                table: "MatchStatsSnapshots",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_ApiFootballId",
                table: "Venues",
                column: "ApiFootballId");

            migrationBuilder.CreateIndex(
                name: "IX_Venues_Name",
                table: "Venues",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Venues_VenueId",
                table: "Matches",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Venues_VenueId",
                table: "Teams",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Venues_VenueId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Venues_VenueId",
                table: "Teams");

            migrationBuilder.DropTable(
                name: "ExternalSyncStates");

            migrationBuilder.DropTable(
                name: "MatchEventsSnapshots");

            migrationBuilder.DropTable(
                name: "MatchHeadToHeadSnapshots");

            migrationBuilder.DropTable(
                name: "MatchLineupSnapshots");

            migrationBuilder.DropTable(
                name: "MatchStatsSnapshots");

            migrationBuilder.DropTable(
                name: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Teams_VenueId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Matches_VenueId",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "ApiFootballEnabled",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "ApiFootballLeagueId",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "ApiFootballSeason",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "FoundedYear",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "VenueCity",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "Matches");

            migrationBuilder.RenameColumn(
                name: "LogoUrl",
                table: "Teams",
                newName: "FlagUrl");

            migrationBuilder.RenameColumn(
                name: "VenueName",
                table: "Matches",
                newName: "Venue");

            migrationBuilder.AlterColumn<int>(
                name: "Role",
                table: "Users",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "IsBanned",
                table: "Users",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "GroupName",
                table: "Teams",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Teams",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PointsEarned",
                table: "Predictions",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "IsScored",
                table: "Predictions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "ResultVersion",
                table: "Matches",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);
        }
    }
}
