using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class RoleEditorViewModel(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<Role, RoleEntity>(unitOfWork, mapper, regionManager)
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            base.OnNavigatedTo(navigationContext);

            var permissionRepository = UnitOfWork.Repository<PermissionEntity>();
            var permissionQuery = permissionRepository.MultipleResultQuery();
            var permissions = await permissionRepository.SearchAsync(permissionQuery);
            if (permissions != null)
            {
                _permissionEntities = permissions;
                Permissions = Mapper.Map<ObservableCollection<Permission>>(_permissionEntities);
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

    protected override async Task<RoleEntity> MapToEntityAsync(Role model)
    {
        var entity =  await Task.FromResult(Mapper.Map<RoleEntity>(model));
        await MapToEntityAsync(entity);
        return entity;
    }


    protected override async Task MapToEntityAsync(Role model, RoleEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
        await MapToEntityAsync(entity);
    }

    private async Task MapToEntityAsync(RoleEntity entity)
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

    private IList<PermissionEntity> _permissionEntities = [];

    private ObservableCollection<Permission> _permissions = [];
    public ObservableCollection<Permission> Permissions
    {
        get => _permissions;
        set => SetProperty(ref _permissions, value);
    }
       
}
