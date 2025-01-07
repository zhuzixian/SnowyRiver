using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Services;

namespace SnowyRiver.Accounts.Modules.Manager.Services;

public class AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper)
    : AuthenticationService<Team, Role, User, Permission,
        Domain.Entities.Team, Domain.Entities.Role, Domain.Entities.User, Domain.Entities.Permission>(unitOfWork, mapper),
        IAuthenticationService;
