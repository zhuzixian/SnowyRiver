using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionsManagerViewModel(IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager): PermissionsManagerViewModelBase<User, UserEntity, Role, RoleEntity, Team, TeamEntity, Permission, PermissionEntity>(unitOfWorkFactory, mapper, dialog, regionManager)
{
}
