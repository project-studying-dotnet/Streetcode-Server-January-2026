using AutoMapper;
using Streetcode.Auth.BLL.DTO.Auth;
using Streetcode.Auth.BLL.DTO.Users;
using Streetcode.Auth.DAL.Entities;

namespace Streetcode.Auth.BLL.Mapping
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            CreateMap<RegisterRequestDTO, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            CreateMap<ApplicationUser, UserDTO>();
        }
    }
}
