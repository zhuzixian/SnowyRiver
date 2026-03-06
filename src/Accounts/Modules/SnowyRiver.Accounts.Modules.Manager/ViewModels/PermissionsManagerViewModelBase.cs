using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionsManagerViewModelBase<TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager)
    : ManagerViewModel<TPermission, TPermissionEntity>(unitOfWorkFactory, mapper, dialog, regionManager)
    where TPermission : Permission, new()
    where TPermissionEntity : PermissionEntity
{
    protected override string EditorView => ViewNames.PermissionEditorView;
    protected override string ManagerViewRegion => RegionNames.PermissionsManagerViewRegion;
}
