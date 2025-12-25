using Application.Features.Venues.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Venues.Mapping
{
    public sealed class VenueProfile : Profile
    {
        public VenueProfile()
        {
            CreateMap<Venue, VenueDto>();
        }
    }
}
