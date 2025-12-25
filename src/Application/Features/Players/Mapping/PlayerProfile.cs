using Application.Features.Players.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Players.Mapping
{
    public sealed class PlayerProfile : Profile
    {
        public PlayerProfile()
        {
            CreateMap<Player, PlayerDto>();

            CreateMap<Player, PlayerWithTeamDto>()
                .ForMember(d => d.TeamName, o => o.MapFrom(s => s.Team.Name))
                .ForMember(d => d.TeamDisplayName, o => o.MapFrom(s => s.Team.DisplayName))
                .ForMember(d => d.TeamLogoUrl, o => o.MapFrom(s => s.Team.LogoUrl));
        }
    }
}
