using AutoMapper;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;

namespace SnowyRiver.Accounts.Modules.Manager;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<User, Domain.Entities.User>()
            .ReverseMap();
        CreateMap<Team, Domain.Entities.Team>()
            .ReverseMap();
        CreateMap<Role, Domain.Entities.Role>()
            .ReverseMap();
        CreateMap<Permission, Domain.Entities.Permission>()
            .ReverseMap();
    }
}
