using Application.Features.Predictions.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Predictions.Mapping
{
    public sealed class BonusPredictionProfile : Profile
    {
        public BonusPredictionProfile()
        {
            CreateMap<BonusPrediction, BonusPredictionDto>();
        }
    }
}
