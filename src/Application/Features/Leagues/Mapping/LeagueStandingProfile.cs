using Application.Features.Leagues.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Leagues.Mapping
{

    public sealed class LeagueStandingProfile : Profile
    {
        public LeagueStandingProfile()
        {
            CreateMap<LeagueStanding, LeagueStandingDto>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.User.Username));
        }
    }
}
