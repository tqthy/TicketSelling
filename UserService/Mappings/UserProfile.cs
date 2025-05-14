using AutoMapper;
using UserService.Data;
using UserService.Models;

namespace UserService.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ApplicationUser, UserDto>();
        // Mapping from RegisterDto to ApplicationUser 
        CreateMap<RegisterDto, ApplicationUser>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));
        CreateMap<UpdateUserDto, ApplicationUser>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
    

}