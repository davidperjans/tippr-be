using Application.Common;
using Application.Features.Tournaments.Commands.CreateTournament;
using Application.Features.Tournaments.DTOs;
using Application.Features.Tournaments.Queries.GetAllTournaments;
using Application.Features.Tournaments.Queries.GetTournamentById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    [Route("api/tournaments")]
    public class TournamentsController : BaseApiController
    {
        private readonly ISender _mediator;
        public TournamentsController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize]
        public async Task<ActionResult<Result<IReadOnlyList<TournamentDto>>>> GetAll([FromQuery] bool onlyActive = false, CancellationToken ct = default)
        {
            var query = new GetAllTournamentsQuery(onlyActive);
            var result = await _mediator.Send(query);

            return FromResult(result);
        }

        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<ActionResult<Result<TournamentDto>>> GetAll(Guid id, CancellationToken ct)
        {
            var query = new GetTournamentByIdQuery(id);
            var result = await _mediator.Send(query);

            return FromResult(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<Result<Guid>>> Create(CreateTournamentCommand command, CancellationToken ct)
        {
            var result = await _mediator.Send(command);

            return FromResult(result);
        }
    }
}
