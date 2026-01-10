using AutoMapper;

using Tilework.Persistence.IdentityManagement.Models;
using Tilework.IdentityManagement.Models;

namespace Tilework.LoadBalancing.Mappers;

public class IdentityManagementMappingProfile : Profile
{
    public IdentityManagementMappingProfile()
    {
        CreateMap<User, UserDTO>().ReverseMap();
    }
}
