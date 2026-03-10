using EntityFrameworkCore.UnitOfWork.Interfaces;
using Mapster;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;

public class RoleEditorViewModelBase<
    TUser, TUserEntity,
    TRole, TRoleEntity,
    TTeam, TTeamEntity,
    TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<TRole, TRoleEntity>(unitOfWorkFactory, mapper, regionManager)
    where TUserEntity : SnowyRiver.Accounts.Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUser : User<TUser, TRole, TTeam, TPermission>, new()
    where TRoleEntity : SnowyRiver.Accounts.Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
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
            base.OnNavigatedTo(navigationContext);

            using var unitOfWork = UnitOfWorkFactory.Create();
            var permissionRepository = unitOfWork.Repository<TPermissionEntity>();
            var permissionQuery = permissionRepository.MultipleResultQuery();
            var permissions = await permissionRepository.SearchAsync(permissionQuery);
            if (permissions != null)
            {
                Permissions = await Mapper.From(permissions)
                    .AdaptToTypeAsync<ObservableCollection<TPermission>>();
                foreach (var permission in Permissions)
                {
                    permission.IsSelected = Model.Permissions.Any(x => x.Id == permission.Id);
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
        var permissionEntities = await GetPermissionEntitiesAsync(unitOfWork, cancellationToken);
        var entity = await mapper.From(Model)
            .AdaptToTypeAsync<TRoleEntity>();
        entity.Permissions = permissionEntities.ToList();
        await unitOfWork.Repository<TRoleEntity>().AddAsync(entity, cancellationToken);
    }

    protected override async Task UpdateAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var permissionEntities = await GetPermissionEntitiesAsync(unitOfWork, cancellationToken);
        var roleRepository = unitOfWork.Repository<TRoleEntity>();
        var entity = await roleRepository.SingleOrDefaultAsync(roleRepository.SingleResultQuery()
                .Include(x => x.Include(e => e.Permissions))
                .AndFilter(x => x.Id == Model.Id), cancellationToken);
        if (entity != null)
        {
            entity.Name = Model.Name;
            entity.Permissions.Clear();
            entity.Permissions.AddRange(permissionEntities);
        }
    }

    private async Task<IList<TPermissionEntity>> GetPermissionEntitiesAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var permissionRepository = unitOfWork.Repository<TPermissionEntity>();
        var selectedPermissionIds = Permissions.Where(p => p.IsSelected)
            .Select(p => p.Id)
            .Distinct();
        var permissionEntities = await permissionRepository.SearchAsync(permissionRepository.MultipleResultQuery()
        .AndFilter(x => selectedPermissionIds.Contains(x.Id)), cancellationToken);
        return permissionEntities;
    }

    public ObservableCollection<TPermission> Permissions
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
