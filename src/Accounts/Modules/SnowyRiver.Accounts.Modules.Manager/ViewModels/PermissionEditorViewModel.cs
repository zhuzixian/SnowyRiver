using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using PermissionEntity = SnowyRiver.Accounts.Domain.Entities.Permission;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionEditorViewModel(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<Permission, PermissionEntity>(unitOfWork, mapper, regionManager)
{
    protected override async Task MapToEntityAsync(Permission model, PermissionEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
    }
}
