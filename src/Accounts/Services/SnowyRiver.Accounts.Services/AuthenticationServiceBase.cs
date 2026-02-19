using EntityFrameworkCore.QueryBuilder.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.ComponentModel.NotifyPropertyChanged;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Services;
public class AuthenticationService<TTeam, TRole, TUser, TPermission,
    TTeamEntity, TRoleEntity, TUserEntity, TPermissionEntity>(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper)
    : NotifyPropertyChangedObject, IAuthenticationService<TUser, TTeam, TRole, TPermission>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TTeamEntity : Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUserEntity : Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRoleEntity : Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermissionEntity : Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
{
    public virtual async Task<(bool, LoginFailedReason)> LoginAsync(string username, string password, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var unitOfWork = unitOfWorkFactory.Create();
            var repository = unitOfWork.Repository<TUserEntity>();
            var query = GetQuery(username);
            var matchedNameUsers = await repository.SearchAsync(query, cancellationToken);
            if (matchedNameUsers.Count > 0)
            {
                var matchedUser = matchedNameUsers.FirstOrDefault(
                    x => PasswordHelper.VerifyPassword(password, x.PasswordSalt, x.Password));
                if (matchedUser != null)
                {
                    User = await mapper.From(matchedUser)
                        .AdaptToTypeAsync<TUser>();
                    IsAuthenticated = true;
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

    protected virtual IMultipleResultQuery<TUserEntity> GetQuery(string username)
    {
        using var unitOfWork = unitOfWorkFactory.Create();
        return unitOfWork.Repository<TUserEntity>().MultipleResultQuery()
            .Include(x => x.Include(user => user.Teams)
                .Include(user => user.Roles)
                .ThenInclude(role => role.Permissions))
            .AndFilter(user => user.Name == username);
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

    private TUser? _user;

    public TUser? User
    {
        get => _user;
        protected set => Set(ref _user, value);
    }

    private TTeam? _selectedTeam;
    public TTeam? SelectedTeam
    {
        get => _selectedTeam;
        set => Set(ref _selectedTeam, value);
    }
}
