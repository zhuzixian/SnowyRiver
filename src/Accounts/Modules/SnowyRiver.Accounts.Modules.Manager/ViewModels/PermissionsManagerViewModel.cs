using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionsManagerViewModel(IUnitOfWork unitOfWork, 
    IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager): ManagerViewModel<Permission, PermissionEntity>(unitOfWork, mapper, dialog, regionManager)
{

    protected override string EditorView => ViewNames.PermissionEditorView;
    protected override string ManagerViewRegion => RegionNames.PermissionsManagerViewRegion;
}
