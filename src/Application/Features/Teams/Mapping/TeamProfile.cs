using Application.Features.Teams.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Teams.Mapping
{
    public sealed class TeamProfile : Profile
    {
        public TeamProfile()
        {
            CreateMap<Team, TeamDto>();
        }
    }
}
