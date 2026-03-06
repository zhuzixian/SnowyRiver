using System.Threading.Tasks;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionEditorViewModelBase<TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<TPermission, TPermissionEntity>(unitOfWorkFactory, mapper, regionManager)
    where TPermission: Permission,new()
    where TPermissionEntity : PermissionEntity
{
    protected override async Task MapToEntityAsync(TPermission model, TPermissionEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
    }
}
