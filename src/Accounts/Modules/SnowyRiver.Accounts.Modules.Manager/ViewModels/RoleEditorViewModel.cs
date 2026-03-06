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
public class RoleEditorViewModel(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : RoleEditorViewModelBase<Role, RoleEntity, Permission, PermissionEntity>(
        unitOfWorkFactory, mapper, regionManager)
{
}
