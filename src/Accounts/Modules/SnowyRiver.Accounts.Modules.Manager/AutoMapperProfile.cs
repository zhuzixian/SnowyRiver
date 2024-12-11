using AutoMapper;

namespace SnowyRiver.Accounts.Modules.Manager;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Models.User, Domain.Entities.User>()
            .ReverseMap();
        CreateMap<Models.Team, Domain.Entities.Team>()
            .ReverseMap();
        CreateMap<Models.Role, Domain.Entities.Role>()
            .ReverseMap();
        CreateMap<Models.Permission, Domain.Entities.Permission>()
            .ReverseMap();
    }
}
