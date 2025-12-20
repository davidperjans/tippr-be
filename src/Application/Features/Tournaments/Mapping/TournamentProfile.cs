using Application.Features.Tournaments.Commands.CreateTournament;
using Application.Features.Tournaments.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Tournaments.Mapping
{
    public class TournamentProfile : Profile
    {
        public TournamentProfile()
        {
            CreateMap<Tournament, TournamentDto>()
                .ForMember(d => d.Country, o => o.Ignore());

            CreateMap<CreateTournamentCommand, Tournament>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.Teams, o => o.Ignore())
                .ForMember(d => d.Matches, o => o.Ignore())
                .ForMember(d => d.BonusQuestions, o => o.Ignore())
                .ForMember(d => d.Leagues, o => o.Ignore())
                .ForMember(d => d.Countries, o => o.Ignore());
        }
    }
}
