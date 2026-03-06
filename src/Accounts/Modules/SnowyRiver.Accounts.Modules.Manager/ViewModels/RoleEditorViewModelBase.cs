using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

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
                _permissionEntities = permissions;
                Permissions = await Mapper.From(_permissionEntities)
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

    protected override async Task<TRoleEntity> MapToEntityAsync(TRole model)
    {
        var entity = await Mapper.From(model)
            .AdaptToTypeAsync<TRoleEntity>();
        await MapToEntityAsync(entity);
        return entity;
    }


    protected override async Task MapToEntityAsync(TRole model, TRoleEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
        await MapToEntityAsync(entity);
    }

    private async Task MapToEntityAsync(TRoleEntity entity)
    {
        entity.Permissions ??= [];
        entity.Permissions.Clear();
        foreach (var permission in Permissions)
        {
            if (permission.IsSelected)
            {
                var permissionEntity = _permissionEntities.First(x => x.Id == permission.Id);
                entity.Permissions ??= [];
                if (entity.Permissions.All(x => x.Id != permissionEntity.Id))
                {
                    entity.Permissions.Add(permissionEntity);
                }

                permissionEntity.Roles ??= [];
                if (permissionEntity.Roles.All(x => x.Id != entity.Id))
                {
                    permissionEntity.Roles.Add(entity);
                }
            }
        }

        await Task.CompletedTask;
    }

    private IList<TPermissionEntity> _permissionEntities = [];

    public ObservableCollection<TPermission> Permissions
    {
        get;
        set => SetProperty(ref field, value);
    } = [];
}
