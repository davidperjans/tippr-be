using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tournaments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tournaments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Teams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FlagUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GroupName = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    ApiFootballId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Teams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Teams_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonusQuestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    QuestionType = table.Column<string>(type: "text", nullable: false),
                    Question = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AnswerTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnswerText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusQuestions_Teams_AnswerTeamId",
                        column: x => x.AnswerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonusQuestions_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Matches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: true),
                    AwayScore = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ApiFootballId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Teams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matches_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    AvatarUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FavoriteTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Teams_FavoriteTeamId",
                        column: x => x.FavoriteTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Leagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    InviteCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsGlobal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MaxMembers = table.Column<int>(type: "integer", nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leagues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leagues_Tournaments_TournamentId",
                        column: x => x.TournamentId,
                        principalTable: "Tournaments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leagues_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BonusPredictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    BonusQuestionId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AnswerTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AnswerText = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PointsEarned = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BonusPredictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BonusPredictions_BonusQuestions_BonusQuestionId",
                        column: x => x.BonusQuestionId,
                        principalTable: "BonusQuestions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonusPredictions_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BonusPredictions_Teams_AnswerTeamId",
                        column: x => x.AnswerTeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_BonusPredictions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    IsEdited = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EditedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatMessages_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeagueMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsMuted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueMembers_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueMembers_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeagueSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    PredictionMode = table.Column<string>(type: "text", nullable: false),
                    DeadlineMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 60),
                    PointsCorrectScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 7),
                    PointsCorrectOutcome = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    PointsCorrectGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    PointsRoundOf16Team = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    PointsQuarterFinalTeam = table.Column<int>(type: "integer", nullable: false, defaultValue: 4),
                    PointsSemiFinalTeam = table.Column<int>(type: "integer", nullable: false, defaultValue: 6),
                    PointsFinalTeam = table.Column<int>(type: "integer", nullable: false, defaultValue: 8),
                    PointsTopScorer = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    PointsWinner = table.Column<int>(type: "integer", nullable: false, defaultValue: 20),
                    PointsMostGoalsGroup = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    PointsMostConcededGroup = table.Column<int>(type: "integer", nullable: false, defaultValue: 10),
                    AllowLateEdits = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueSettings_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeagueStandings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BonusPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    PreviousRank = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueStandings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeagueStandings_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Predictions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Predictions_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Predictions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BonusPredictions_AnswerTeamId",
                table: "BonusPredictions",
                column: "AnswerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusPredictions_BonusQuestionId",
                table: "BonusPredictions",
                column: "BonusQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusPredictions_LeagueId",
                table: "BonusPredictions",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusPredictions_UserId",
                table: "BonusPredictions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusPredictions_UserId_BonusQuestionId_LeagueId",
                table: "BonusPredictions",
                columns: new[] { "UserId", "BonusQuestionId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BonusQuestions_AnswerTeamId",
                table: "BonusQuestions",
                column: "AnswerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_BonusQuestions_TournamentId",
                table: "BonusQuestions",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CreatedAt",
                table: "ChatMessages",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_LeagueId",
                table: "ChatMessages",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_LeagueId_CreatedAt",
                table: "ChatMessages",
                columns: new[] { "LeagueId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_UserId",
                table: "ChatMessages",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_LeagueId",
                table: "LeagueMembers",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_LeagueId_UserId",
                table: "LeagueMembers",
                columns: new[] { "LeagueId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueMembers_UserId",
                table: "LeagueMembers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_InviteCode",
                table: "Leagues",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_OwnerId",
                table: "Leagues",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leagues_TournamentId",
                table: "Leagues",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueSettings_LeagueId",
                table: "LeagueSettings",
                column: "LeagueId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_LeagueId",
                table: "LeagueStandings",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_LeagueId_Rank",
                table: "LeagueStandings",
                columns: new[] { "LeagueId", "Rank" });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_LeagueId_UserId",
                table: "LeagueStandings",
                columns: new[] { "LeagueId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeagueStandings_UserId",
                table: "LeagueStandings",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_AwayTeamId",
                table: "Matches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_HomeTeamId",
                table: "Matches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_MatchDate",
                table: "Matches",
                column: "MatchDate");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_Status",
                table: "Matches",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TournamentId",
                table: "Matches",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Matches_TournamentId_Status_MatchDate",
                table: "Matches",
                columns: new[] { "TournamentId", "Status", "MatchDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_LeagueId",
                table: "Predictions",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_MatchId",
                table: "Predictions",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId",
                table: "Predictions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId_LeagueId",
                table: "Predictions",
                columns: new[] { "UserId", "LeagueId" });

            migrationBuilder.CreateIndex(
                name: "IX_Predictions_UserId_MatchId_LeagueId",
                table: "Predictions",
                columns: new[] { "UserId", "MatchId", "LeagueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_Code",
                table: "Teams",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TournamentId",
                table: "Teams",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FavoriteTeamId",
                table: "Users",
                column: "FavoriteTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BonusPredictions");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "LeagueMembers");

            migrationBuilder.DropTable(
                name: "LeagueSettings");

            migrationBuilder.DropTable(
                name: "LeagueStandings");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Predictions");

            migrationBuilder.DropTable(
                name: "BonusQuestions");

            migrationBuilder.DropTable(
                name: "Leagues");

            migrationBuilder.DropTable(
                name: "Matches");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Teams");

            migrationBuilder.DropTable(
                name: "Tournaments");
        }
    }
}
