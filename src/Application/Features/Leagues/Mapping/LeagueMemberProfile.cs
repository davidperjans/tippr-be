using Application.Features.Auth.DTOs;
using Application.Features.Leagues.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Leagues.Mapping
{
    public class LeagueMemberProfile : Profile
    {
        public LeagueMemberProfile()
        {
            CreateMap<LeagueMember, LeagueMemberDto>()
                .ForMember(d => d.User, o => o.MapFrom(s => s.User));
        }
    }
}
