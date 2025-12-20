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
                 .ForMember(d => d.Username, o => o.MapFrom(s => s.User.Username))
                 .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.User.AvatarUrl))

                 // Rank is guaranteed to be >= 1
                 .ForMember(d => d.Rank, o => o.MapFrom(s => s.Rank))

                 // RankChange only depends on PreviousRank now
                 .ForMember(d => d.RankChange, o => o.MapFrom(s =>
                     s.PreviousRank.HasValue
                         ? s.PreviousRank.Value - s.Rank
                         : (int?)null));
        }
    }
}
