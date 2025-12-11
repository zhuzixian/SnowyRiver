using MapsterMapper;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Services;

public class AuthenticationService(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
    : AuthenticationService<Team, Role, User, Permission,
            Domain.Entities.Team, Domain.Entities.Role, Domain.Entities.User, Domain.Entities.Permission>(unitOfWorkFactory, mapper),
        IAuthenticationService;
