using Application.Features.Predictions.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Predictions.Mapping
{
    public sealed class PredictionProfile : Profile
    {
        public PredictionProfile()
        {
            CreateMap<Prediction, PredictionDto>();
        }
    }
}
