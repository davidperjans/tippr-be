using Application.Common;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Queries.GetMe
{
    public record GetCurrentUserQuery(
        Guid UserId
    ) : IRequest<Result<CurrentUserResponse>>;
}
