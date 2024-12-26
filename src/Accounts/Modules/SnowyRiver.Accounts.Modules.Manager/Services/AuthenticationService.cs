using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.Modules.Manager.Models;
using SnowyRiver.WPF.NotifyPropertyChangedBase;

namespace SnowyRiver.Accounts.Modules.Manager.Services;
public class AuthenticationService(IUnitOfWork unitOfWork, IMapper mapper): NotifyPropertyChangedObject, IAuthenticationService
{
    public async Task<(bool, LoginFailedReason)> LoginAsync(string username, string password)
    {
        try
        {
            var repository = unitOfWork.Repository<Domain.Entities.User>();
            var query = repository.MultipleResultQuery()
                .Include(x => x.Include(user => user.Teams))
                .Include(x => x.Include(user => user.Roles)
                    .ThenInclude(role => role.Permissions))
                .AndFilter(user => user.Name == username);
            var matchedNameUsers = await repository.SearchAsync(query);
            if (matchedNameUsers.Count > 0)
            {
                var matchedUser = matchedNameUsers.FirstOrDefault(
                    x => PasswordHelper.VerifyPassword(password, x.PasswordSalt, x.Password));
                if (matchedUser != null)
                {
                    User = mapper.Map<User>(matchedUser);
                    return (true, LoginFailedReason.Succeed);
                }

                return (false, LoginFailedReason.PasswordVerificationFailed);
            }

            return (false, LoginFailedReason.NotFoundUser);
        }
        catch (Exception e)
        {
            return (false, LoginFailedReason.UnKnow);
        }
    }

    public async Task LogoutAsync()
    {
        IsAuthenticated = false;
        await Task.CompletedTask;
    }

    private bool _isAuthenticated;

    public bool IsAuthenticated
    {
        get => _isAuthenticated;
        set => Set(ref _isAuthenticated, value);
    }

    private User? _user;

    public User? User
    {
        get => _user;
        protected set => Set(ref _user, value);
    }

    private Team? _selectedTeam;
    public Team? SelectedTeam
    {
        get => _selectedTeam;
        set => Set(ref _selectedTeam, value);
    }
}
