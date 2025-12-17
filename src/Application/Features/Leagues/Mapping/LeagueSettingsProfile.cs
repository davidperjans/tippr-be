using Application.Features.Leagues.Commands.UpdateLeagueSettings;
using Application.Features.Leagues.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Leagues.Mapping
{
    public sealed class LeagueSettingsProfile : Profile
    {
        public LeagueSettingsProfile()
        {
            // Entity -> DTO
            CreateMap<LeagueSettings, LeagueSettingsDto>()
                .ForMember(d => d.PredictionMode, o => o.MapFrom(s => s.PredictionMode.ToString()));

            // Command -> Entity (för update)
            CreateMap<UpdateLeagueSettingsCommand, LeagueSettings>()
                .ForMember(d => d.Id, o => o.Ignore())
                .ForMember(d => d.LeagueId, o => o.Ignore())      // sätts vid create / ska inte ändras
                .ForMember(d => d.CreatedAt, o => o.Ignore())     // om du har timestamps
                .ForMember(d => d.UpdatedAt, o => o.Ignore());    // sätter vi manuellt i handler
        }
    }
}
