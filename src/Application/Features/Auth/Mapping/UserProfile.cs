using Application.Features.Auth.DTOs;
using AutoMapper;
using Domain.Entities;

namespace Application.Features.Auth.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
        }
    }
}
