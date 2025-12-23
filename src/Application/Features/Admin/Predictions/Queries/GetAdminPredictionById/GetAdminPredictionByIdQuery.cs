using Application.Common;
using Application.Features.Admin.DTOs;
using MediatR;

namespace Application.Features.Admin.Predictions.Queries.GetAdminPredictionById
{
    public sealed record GetAdminPredictionByIdQuery(Guid PredictionId) : IRequest<Result<AdminPredictionDto>>;
}
