using Application.Common;
using Application.Common.Interfaces;
using Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Features.Admin.ApiFootball.Commands.MergeDuplicateTeams
{
    public class MergeDuplicateTeamsCommandHandler
        : IRequestHandler<MergeDuplicateTeamsCommand, Result<MergeDuplicateTeamsResult>>
    {
        private readonly ITipprDbContext _db;
        private readonly ILogger<MergeDuplicateTeamsCommandHandler> _logger;

        public MergeDuplicateTeamsCommandHandler(
            ITipprDbContext db,
            ILogger<MergeDuplicateTeamsCommandHandler> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Result<MergeDuplicateTeamsResult>> Handle(
            MergeDuplicateTeamsCommand request,
            CancellationToken ct)
        {
            var tournament = await _db.Tournaments
                .FirstOrDefaultAsync(t => t.Id == request.TournamentId, ct);

            if (tournament == null)
                return Result<MergeDuplicateTeamsResult>.NotFound("Tournament not found", "admin.tournament_not_found");

            // Find all teams for this tournament
            var allTeams = await _db.Teams
                .Where(t => t.TournamentId == request.TournamentId)
                .ToListAsync(ct);

            // Old teams = no ApiFootballId (these are the Swedish-named ones with FIFA data)
            var oldTeams = allTeams.Where(t => !t.ApiFootballId.HasValue).ToList();

            // New teams = have ApiFootballId (these are the English-named ones from API)
            var newTeams = allTeams.Where(t => t.ApiFootballId.HasValue).ToList();

            if (!oldTeams.Any())
            {
                return Result<MergeDuplicateTeamsResult>.Success(new MergeDuplicateTeamsResult
                {
                    WasDryRun = request.DryRun,
                    MergeActions = new List<MergeAction>()
                });
            }

            var mergeActions = new List<MergeAction>();
            var matchesUpdated = 0;
            var predictionsUpdated = 0;
            var favoritesUpdated = 0;
            var bonusQuestionsUpdated = 0;
            var bonusPredictionsUpdated = 0;

            foreach (var oldTeam in oldTeams)
            {
                // Try to find matching new team
                Team? newTeam = null;

                if (!string.IsNullOrWhiteSpace(oldTeam.Code))
                {
                    // Try exact code match first
                    newTeam = newTeams.FirstOrDefault(t =>
                        !string.IsNullOrWhiteSpace(t.Code) &&
                        t.Code.Equals(oldTeam.Code, StringComparison.OrdinalIgnoreCase));

                    // If no match and old code is 2 letters, try mapping to 3-letter code
                    if (newTeam == null && oldTeam.Code.Length == 2)
                    {
                        var threeLetterCode = MapTwoLetterToThreeLetter(oldTeam.Code.ToUpperInvariant());
                        if (!string.IsNullOrEmpty(threeLetterCode))
                        {
                            newTeam = newTeams.FirstOrDefault(t =>
                                !string.IsNullOrWhiteSpace(t.Code) &&
                                t.Code.Equals(threeLetterCode, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                }

                if (newTeam == null)
                {
                    _logger.LogWarning(
                        "Could not find matching new team for old team '{OldTeamName}' (Code: {Code}). Skipping.",
                        oldTeam.Name, oldTeam.Code);
                    continue;
                }

                _logger.LogInformation(
                    "Matched old team '{OldTeamName}' -> new team '{NewTeamName}' by Code '{Code}'",
                    oldTeam.Name, newTeam.Name, oldTeam.Code);

                var action = new MergeAction
                {
                    OldTeamId = oldTeam.Id,
                    OldTeamName = oldTeam.Name,
                    NewTeamId = newTeam.Id,
                    NewTeamName = newTeam.Name,
                    TransferredDisplayName = oldTeam.Name,
                    TransferredFifaRank = oldTeam.FifaRank,
                    TransferredFifaPoints = oldTeam.FifaPoints
                };
                mergeActions.Add(action);

                if (!request.DryRun)
                {
                    // Transfer data from old team to new team
                    newTeam.DisplayName = oldTeam.Name;  // Swedish name becomes DisplayName
                    newTeam.FifaRank = oldTeam.FifaRank ?? newTeam.FifaRank;
                    newTeam.FifaPoints = oldTeam.FifaPoints ?? newTeam.FifaPoints;
                    newTeam.FifaRankingUpdatedAt = oldTeam.FifaRankingUpdatedAt ?? newTeam.FifaRankingUpdatedAt;
                    newTeam.GroupId = oldTeam.GroupId ?? newTeam.GroupId;
                    newTeam.UpdatedAt = DateTime.UtcNow;

                    // Update Match references
                    var homeMatches = await _db.Matches
                        .Where(m => m.HomeTeamId == oldTeam.Id)
                        .ToListAsync(ct);
                    foreach (var match in homeMatches)
                    {
                        match.HomeTeamId = newTeam.Id;
                        match.UpdatedAt = DateTime.UtcNow;
                        matchesUpdated++;
                    }

                    var awayMatches = await _db.Matches
                        .Where(m => m.AwayTeamId == oldTeam.Id)
                        .ToListAsync(ct);
                    foreach (var match in awayMatches)
                    {
                        match.AwayTeamId = newTeam.Id;
                        match.UpdatedAt = DateTime.UtcNow;
                        matchesUpdated++;
                    }

                    // Update User.FavoriteTeamId references
                    var usersWithFavorite = await _db.Users
                        .Where(u => u.FavoriteTeamId == oldTeam.Id)
                        .ToListAsync(ct);
                    foreach (var user in usersWithFavorite)
                    {
                        user.FavoriteTeamId = newTeam.Id;
                        user.UpdatedAt = DateTime.UtcNow;
                        favoritesUpdated++;
                    }

                    // Update BonusQuestion.AnswerTeamId references
                    var bonusQuestions = await _db.BonusQuestions
                        .Where(bq => bq.AnswerTeamId == oldTeam.Id)
                        .ToListAsync(ct);
                    foreach (var bq in bonusQuestions)
                    {
                        bq.AnswerTeamId = newTeam.Id;
                        bq.UpdatedAt = DateTime.UtcNow;
                        bonusQuestionsUpdated++;
                    }

                    // Update BonusPrediction.AnswerTeamId references
                    var bonusPredictions = await _db.BonusPredictions
                        .Where(bp => bp.AnswerTeamId == oldTeam.Id)
                        .ToListAsync(ct);
                    foreach (var bp in bonusPredictions)
                    {
                        bp.AnswerTeamId = newTeam.Id;
                        bp.UpdatedAt = DateTime.UtcNow;
                        bonusPredictionsUpdated++;
                    }

                    // Delete old team
                    _db.Teams.Remove(oldTeam);

                    _logger.LogInformation(
                        "Merged team '{OldTeamName}' -> '{NewTeamName}'. " +
                        "Transferred: DisplayName='{DisplayName}', FifaRank={FifaRank}, FifaPoints={FifaPoints}",
                        oldTeam.Name, newTeam.Name, newTeam.DisplayName, newTeam.FifaRank, newTeam.FifaPoints);
                }
            }

            if (!request.DryRun)
            {
                await _db.SaveChangesAsync(ct);
            }

            var result = new MergeDuplicateTeamsResult
            {
                TeamsMerged = mergeActions.Count,
                TeamsDeleted = request.DryRun ? 0 : mergeActions.Count,
                MatchesUpdated = matchesUpdated,
                PredictionsUpdated = predictionsUpdated,
                FavoritesUpdated = favoritesUpdated,
                WasDryRun = request.DryRun,
                MergeActions = mergeActions
            };

            _logger.LogInformation(
                "Merge duplicate teams {Status} for tournament {TournamentId}. " +
                "Teams merged: {TeamsMerged}, Matches updated: {MatchesUpdated}, Favorites updated: {FavoritesUpdated}",
                request.DryRun ? "DRY RUN" : "COMPLETED",
                request.TournamentId, result.TeamsMerged, result.MatchesUpdated, result.FavoritesUpdated);

            return Result<MergeDuplicateTeamsResult>.Success(result);
        }

        /// <summary>
        /// Maps ISO 3166-1 alpha-2 codes to FIFA/API-Football 3-letter codes.
        /// Covers World Cup 2026 participating nations and common football nations.
        /// </summary>
        private static string? MapTwoLetterToThreeLetter(string twoLetterCode)
        {
            return twoLetterCode switch
            {
                // Europe
                "SE" => "SWE",  // Sweden
                "DE" => "GER",  // Germany
                "FR" => "FRA",  // France
                "ES" => "ESP",  // Spain
                "IT" => "ITA",  // Italy
                "GB" => "ENG",  // England (note: GB is UK, but often used for England)
                "EN" => "ENG",  // England (custom code some use)
                "NL" => "NED",  // Netherlands
                "BE" => "BEL",  // Belgium
                "PT" => "POR",  // Portugal
                "PL" => "POL",  // Poland
                "HR" => "CRO",  // Croatia
                "CH" => "SUI",  // Switzerland
                "AT" => "AUT",  // Austria
                "DK" => "DEN",  // Denmark
                "NO" => "NOR",  // Norway
                "RS" => "SRB",  // Serbia
                "UA" => "UKR",  // Ukraine
                "CZ" => "CZE",  // Czech Republic
                "RO" => "ROU",  // Romania
                "GR" => "GRE",  // Greece
                "SK" => "SVK",  // Slovakia
                "SI" => "SVN",  // Slovenia
                "HU" => "HUN",  // Hungary
                "IE" => "IRL",  // Ireland
                "SC" => "SCO",  // Scotland
                "WA" => "WAL",  // Wales
                "IS" => "ISL",  // Iceland
                "FI" => "FIN",  // Finland
                "BA" => "BIH",  // Bosnia
                "AL" => "ALB",  // Albania
                "MK" => "MKD",  // North Macedonia
                "ME" => "MNE",  // Montenegro
                "TR" => "TUR",  // Turkey (partly Europe)

                // South America
                "BR" => "BRA",  // Brazil
                "AR" => "ARG",  // Argentina
                "CO" => "COL",  // Colombia
                "UY" => "URU",  // Uruguay
                "CL" => "CHI",  // Chile
                "EC" => "ECU",  // Ecuador
                "PY" => "PAR",  // Paraguay
                "PE" => "PER",  // Peru
                "VE" => "VEN",  // Venezuela
                "BO" => "BOL",  // Bolivia

                // North/Central America & Caribbean
                "US" => "USA",  // United States
                "MX" => "MEX",  // Mexico
                "CA" => "CAN",  // Canada
                "CR" => "CRC",  // Costa Rica
                "PA" => "PAN",  // Panama
                "JM" => "JAM",  // Jamaica
                "HN" => "HON",  // Honduras
                "SV" => "SLV",  // El Salvador
                "GT" => "GUA",  // Guatemala

                // Africa
                "MA" => "MAR",  // Morocco
                "SN" => "SEN",  // Senegal
                "TN" => "TUN",  // Tunisia
                "NG" => "NGA",  // Nigeria
                "CM" => "CMR",  // Cameroon
                "GH" => "GHA",  // Ghana
                "CI" => "CIV",  // Ivory Coast
                "EG" => "EGY",  // Egypt
                "DZ" => "ALG",  // Algeria
                "ZA" => "RSA",  // South Africa
                "ML" => "MLI",  // Mali
                "BF" => "BFA",  // Burkina Faso
                "CV" => "CPV",  // Cape Verde

                // Asia
                "JP" => "JPN",  // Japan
                "KR" => "KOR",  // South Korea
                "AU" => "AUS",  // Australia
                "SA" => "KSA",  // Saudi Arabia
                "IR" => "IRN",  // Iran
                "QA" => "QAT",  // Qatar
                "AE" => "UAE",  // UAE
                "CN" => "CHN",  // China
                "IN" => "IND",  // India
                "ID" => "IDN",  // Indonesia
                "TH" => "THA",  // Thailand
                "VN" => "VIE",  // Vietnam
                "UZ" => "UZB",  // Uzbekistan
                "IQ" => "IRQ",  // Iraq
                "JO" => "JOR",  // Jordan
                "KW" => "KUW",  // Kuwait
                "BH" => "BHR",  // Bahrain
                "OM" => "OMA",  // Oman

                // Oceania
                "NZ" => "NZL",  // New Zealand

                _ => null
            };
        }
    }
}
