using AutoMapper;

namespace SnowyRiver.Accounts.Modules.Manager;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Models.User, Domain.Entities.User>()
            .ReverseMap();
        CreateMap<Models.Permission, Domain.Entities.Permission>()
            .ReverseMap();
    }
}
