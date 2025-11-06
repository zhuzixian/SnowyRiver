using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using SnowyRiver.Accounts.Services.Interfaces;

namespace SnowyRiver.Accounts.Services;

public class AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper)
    : AuthenticationService<Team, Role, User, Permission,
            Domain.Entities.Team, Domain.Entities.Role, Domain.Entities.User, Domain.Entities.Permission>(unitOfWork,
            mapper),
        IAuthenticationService;
