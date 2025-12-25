using Application.Features.Matches.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Matches.Mapping
{
    public class MatchProfile : Profile
    {
        public MatchProfile()
        {
            CreateMap<Match, MatchDetailDto>()
                .ForMember(d => d.Venue, o => o.MapFrom(s => s.VenueName));
        }
    }
}
