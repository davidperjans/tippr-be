using Application.Common;
using Application.Common.Interfaces;
using Application.Features.Auth.DTOs;
using MediatR;

namespace Application.Features.Auth.Queries.GetMe
{
    public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
    {
        private readonly IAuthService _authService;
        public GetCurrentUserQueryHandler(IAuthService authService)
        {
            _authService = authService;
        }

        public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            var user = await _authService.GetByAuthUserIdAsync(request.UserId, cancellationToken);

            if (user == null)
                return Result<CurrentUserResponse>.Failure("user not synced");

            await _authService.UpdateLastLoginAsync(user.Id, cancellationToken);

            var resultDto = new CurrentUserResponse(user.Id, user.Email, user.DisplayName, user.LastLoginAt ?? user.CreatedAt);

            return Result<CurrentUserResponse>.Success(resultDto);
        }
    }
}
