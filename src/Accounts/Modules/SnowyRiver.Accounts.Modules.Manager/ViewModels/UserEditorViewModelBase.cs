using EntityFrameworkCore.UnitOfWork.Interfaces;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Domain.Helpers;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UserEditorViewModelBase<
    TUser, TUserEntity, 
    TRole, TRoleEntity,
    TTeam, TTeamEntity,
    TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<TUser, TUserEntity>(unitOfWorkFactory, mapper, regionManager)
    where TUserEntity : SnowyRiver.Accounts.Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUser : User<TUser, TRole, TTeam, TPermission>, new()
    where TRoleEntity: SnowyRiver.Accounts.Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRole : Role<TUser, TRole, TTeam, TPermission>, new()
    where TTeamEntity : SnowyRiver.Accounts.Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>, new()
    where TPermissionEntity : SnowyRiver.Accounts.Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>, new()
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            if (navigationContext.Parameters.TryGetValue<bool>(nameof(TeamsEnable), out var teamsEnable))
            {
                TeamsEnable = teamsEnable;
            }

            base.OnNavigatedTo(navigationContext);

            using var unitOfWork = UnitOfWorkFactory.Create();
            var roleRepository = unitOfWork.Repository<TRoleEntity>();
            var roleQuery = roleRepository.MultipleResultQuery()
                .Include(source => 
                    source.Include(x => x.Users));
            var roles = await roleRepository.SearchAsync(roleQuery);
            if (roles != null)
            {
                Roles = await Mapper.From(roles)
                    .AdaptToTypeAsync<ObservableCollection<TRole>>();
                foreach (var role in Roles)
                {
                    role.IsSelected = Model.Roles.Any(x => x.Id ==  role.Id);
                }
            }

            var teamRepository = unitOfWork.Repository<TTeamEntity>();
            var teamQuery = teamRepository.MultipleResultQuery()
                .Include(source => source.Include(x => x.Users));
            var teams = await teamRepository.SearchAsync(teamQuery);
            if (teams != null)
            {
                Teams = await Mapper.From(teams)
                    .AdaptToTypeAsync<ObservableCollection<TTeam>>();
                foreach (var team in Teams)
                {
                    team.IsSelected = Model.Teams.Any(x => x.Id == team.Id);
                }
            }
        }
        catch (Exception e)
        {
            //
        }
    }

    protected override async Task AddAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var entity = await mapper.From(Model)
            .AdaptToTypeAsync<TUserEntity>();
        await UpdateAsync(unitOfWork, entity, cancellationToken);
        await unitOfWork.Repository<TUserEntity>().AddAsync(entity, cancellationToken);
    }

    protected override async Task UpdateAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var userRepository = unitOfWork.Repository<TUserEntity>();
        var entity = await userRepository.SingleOrDefaultAsync(
            userRepository.SingleResultQuery()
                .Include(x => x.Include(e => e.Teams)
                    .Include(e => e.Roles))
                .AndFilter(x => x.Id == Model.Id), cancellationToken);
        if (entity != null)
        {
            entity.Name = Model.Name;

            await UpdateAsync(unitOfWork,entity,  cancellationToken);
        }
    }

    protected async Task UpdateAsync(IUnitOfWork unitOfWork, TUserEntity entity, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(entity.PasswordSalt))
        {
            entity.PasswordSalt = PasswordHelper.CreateSalt();
        }

        if (!string.IsNullOrWhiteSpace(Model.NewPassword))
        {
            entity.Password = PasswordHelper.CreatePassword(Model.NewPassword, entity.PasswordSalt);
        }

        await UpdateAsync(unitOfWork, entity.Roles, Roles, cancellationToken);
        await UpdateAsync(unitOfWork, entity.Teams, Teams, cancellationToken);
    }

    public bool TeamsEnable
    {
        get;
        set => SetProperty(ref field, value);
    } = true;


    public ObservableCollection<TRole> Roles
    {
        get;
        set => SetProperty(ref field, value);
    } = [];


    public ObservableCollection<TTeam> Teams
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
