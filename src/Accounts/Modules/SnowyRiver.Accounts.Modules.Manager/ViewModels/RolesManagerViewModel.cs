using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class RolesManagerViewModel(
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager) 
    : RolesManagerViewModelBase<Role, RoleEntity>(unitOfWorkFactory, mapper, dialog, regionManager)
{
}
