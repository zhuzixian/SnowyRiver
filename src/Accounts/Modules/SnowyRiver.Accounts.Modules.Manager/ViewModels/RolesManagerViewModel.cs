using System.Threading.Tasks;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
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
    : ManagerViewModel<Role, RoleEntity>(unitOfWorkFactory, mapper, dialog, regionManager)
{
    protected override async Task<IMultipleResultQuery<RoleEntity>> GetQueryAsync(IRepository<RoleEntity> repository)
    {
        var query = await base.GetQueryAsync(repository);
        query = query.Include(source => source.Include(x => x.Permissions));
        return query;
    }

    protected override string EditorView => ViewNames.RoleEditorView;
    protected override string ManagerViewRegion => RegionNames.RolesManagerViewRegion;
}
