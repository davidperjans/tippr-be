using Application.Features.Leagues.Commands.CreateLeague;
using Application.Features.Leagues.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Leagues.Mapping
{
    public class LeagueProfile : Profile
    {
        public LeagueProfile()
        {
            CreateMap<League, LeagueDto>();

            CreateMap<CreateLeagueCommand, League>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.CreatedAt, o => o.Ignore())
                .ForMember(d => d.UpdatedAt, o => o.Ignore())
                .ForMember(d => d.InviteCode, o => o.Ignore())
                .ForMember(d => d.IsGlobal, o => o.Ignore())

                // navigation properties
                .ForMember(d => d.Tournament, o => o.Ignore())
                .ForMember(d => d.Owner, o => o.Ignore())
                .ForMember(d => d.Settings, o => o.Ignore())
                .ForMember(d => d.Members, o => o.Ignore())
                .ForMember(d => d.Predictions, o => o.Ignore())
                .ForMember(d => d.BonusPredictions, o => o.Ignore())
                .ForMember(d => d.Standings, o => o.Ignore())
                .ForMember(d => d.ChatMessages, o => o.Ignore());
        }
    }
}
