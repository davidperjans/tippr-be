using Application.Features.Predictions.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Predictions.Mapping
{
    public sealed class BonusQuestionProfile : Profile
    {
        public BonusQuestionProfile()
        {
            CreateMap<BonusQuestion, BonusQuestionDto>();
        }
    }
}
