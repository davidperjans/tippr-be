using Application.Common;
using Application.Features.Admin.ApiFootball.Commands.MergeDuplicateTeams;
using Application.Features.Admin.ApiFootball.Commands.SyncGroupStandings;
using Application.Features.Admin.ApiFootball.Commands.SyncMatchLineups;
using Application.Features.Admin.ApiFootball.Commands.SyncTeamSquads;
using Application.Features.Admin.ApiFootball.Commands.SyncTournamentBaseline;
using Application.Features.Admin.ApiFootball.Commands.SyncTournamentResults;
using Application.Features.Admin.ApiFootball.Queries.ValidateLeague;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/apifootball")]
    [Authorize(Policy = "AdminOnly")]
    public class AdminApiFootballController : BaseApiController
    {
        private readonly ISender _mediator;

        public AdminApiFootballController(ISender mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Sync tournament baseline data (teams, venues, fixtures) from API-FOOTBALL.
        /// This is the initial sync that maps external data to existing teams/matches.
        /// Use createMissingTeams=true to auto-create teams that don't exist in the database.
        /// </summary>
        [HttpPost("tournaments/{tournamentId:guid}/baseline")]
        public async Task<ActionResult<Result<SyncTournamentBaselineResult>>> SyncTournamentBaseline(
            Guid tournamentId,
            [FromQuery] bool force = false,
            [FromQuery] bool createMissingTeams = false,
            CancellationToken ct = default)
        {
            var command = new SyncTournamentBaselineCommand(tournamentId, force, createMissingTeams);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Sync tournament match results from API-FOOTBALL.
        /// Updates scores and statuses for matches that are live or recently finished.
        /// </summary>
        [HttpPost("tournaments/{tournamentId:guid}/results")]
        public async Task<ActionResult<Result<SyncTournamentResultsResult>>> SyncTournamentResults(
            Guid tournamentId,
            [FromQuery] bool force = false,
            CancellationToken ct = default)
        {
            var command = new SyncTournamentResultsCommand(tournamentId, force);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Sync match lineups from API-FOOTBALL.
        /// Lineups are typically available ~60 minutes before kickoff.
        /// </summary>
        [HttpPost("matches/{matchId:guid}/lineups")]
        public async Task<ActionResult<Result<SyncMatchLineupsResult>>> SyncMatchLineups(
            Guid matchId,
            [FromQuery] bool force = false,
            CancellationToken ct = default)
        {
            var command = new SyncMatchLineupsCommand(matchId, force);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Validate that a league/season combination exists in API-FOOTBALL
        /// and check what data coverage is available.
        /// </summary>
        [HttpGet("leagues/validate")]
        public async Task<ActionResult<Result<ValidateLeagueResult>>> ValidateLeague(
            [FromQuery] int id,
            [FromQuery] int season,
            CancellationToken ct = default)
        {
            var query = new ValidateLeagueQuery(id, season);
            var result = await _mediator.Send(query, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Merge duplicate teams in a tournament.
        /// Finds old teams (without ApiFootballId) and merges them with new teams (with ApiFootballId).
        /// Transfers DisplayName (Swedish name), FifaRank, FifaPoints, and updates all references.
        /// Use dryRun=true (default) to preview changes without applying them.
        /// </summary>
        [HttpPost("tournaments/{tournamentId:guid}/merge-teams")]
        public async Task<ActionResult<Result<MergeDuplicateTeamsResult>>> MergeDuplicateTeams(
            Guid tournamentId,
            [FromQuery] bool dryRun = true,
            CancellationToken ct = default)
        {
            var command = new MergeDuplicateTeamsCommand(tournamentId, dryRun);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Sync group standings from API-FOOTBALL.
        /// Creates groups if they don't exist, assigns teams to groups,
        /// and updates group standings (position, points, goals, etc.).
        /// </summary>
        [HttpPost("tournaments/{tournamentId:guid}/standings")]
        public async Task<ActionResult<Result<SyncGroupStandingsResult>>> SyncGroupStandings(
            Guid tournamentId,
            [FromQuery] bool force = false,
            CancellationToken ct = default)
        {
            var command = new SyncGroupStandingsCommand(tournamentId, force);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }

        /// <summary>
        /// Sync team squads (players) from API-FOOTBALL.
        /// Fetches all players for all teams in the tournament.
        /// Players are used for top scorer predictions and lineup display.
        /// </summary>
        [HttpPost("tournaments/{tournamentId:guid}/squads")]
        public async Task<ActionResult<Result<SyncTeamSquadsResult>>> SyncTeamSquads(
            Guid tournamentId,
            [FromQuery] bool force = false,
            CancellationToken ct = default)
        {
            var command = new SyncTeamSquadsCommand(tournamentId, force);
            var result = await _mediator.Send(command, ct);
            return FromResult(result);
        }
    }
}
