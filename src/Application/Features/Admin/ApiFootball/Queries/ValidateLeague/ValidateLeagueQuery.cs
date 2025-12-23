using Application.Common;
using Application.Common.Interfaces;
using MediatR;

namespace Application.Features.Admin.ApiFootball.Queries.ValidateLeague
{
    public sealed record ValidateLeagueQuery(
        int LeagueId,
        int Season
    ) : IRequest<Result<ValidateLeagueResult>>;

    public sealed class ValidateLeagueResult
    {
        public bool IsValid { get; init; }
        public string? LeagueName { get; init; }
        public string? LeagueType { get; init; }
        public string? Country { get; init; }
        public bool HasLineupsSupport { get; init; }
        public bool HasEventsSupport { get; init; }
        public bool HasStatisticsSupport { get; init; }
        public string? ErrorMessage { get; init; }
    }

    public class ValidateLeagueQueryHandler : IRequestHandler<ValidateLeagueQuery, Result<ValidateLeagueResult>>
    {
        private readonly IApiFootballClient _apiClient;

        public ValidateLeagueQueryHandler(IApiFootballClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<Result<ValidateLeagueResult>> Handle(ValidateLeagueQuery request, CancellationToken ct)
        {
            var result = await _apiClient.ValidateLeagueAsync(request.LeagueId, request.Season, ct);

            if (!result.Success)
            {
                return Result<ValidateLeagueResult>.Failure(
                    $"Failed to validate league: {result.ErrorMessage}");
            }

            return Result<ValidateLeagueResult>.Success(new ValidateLeagueResult
            {
                IsValid = result.IsValid,
                LeagueName = result.LeagueName,
                LeagueType = result.LeagueType,
                Country = result.Country,
                HasLineupsSupport = result.HasLineupsSupport,
                HasEventsSupport = result.HasEventsSupport,
                HasStatisticsSupport = result.HasStatisticsSupport,
                ErrorMessage = result.IsValid ? null : result.ErrorMessage
            });
        }
    }
}
