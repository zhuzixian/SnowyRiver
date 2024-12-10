using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionsManagerViewModel(IUnitOfWork unitOfWork, 
    IMapper mapper,
    IRegionManager regionManager): ManagerViewModel<Permission, PermissionEntity>(unitOfWork, mapper, regionManager)
{

    protected override string EditorView => ViewNames.PermissionEditorView;
    protected override string ManagerViewRegion => RegionNames.PermissionsManagerViewRegion;
}
