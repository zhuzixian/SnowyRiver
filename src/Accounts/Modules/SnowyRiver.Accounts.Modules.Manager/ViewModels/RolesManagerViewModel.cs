using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using RoleEntity = SnowyRiver.Accounts.Domain.Entities.Role;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class RolesManagerViewModel(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IRegionManager regionManager) 
    : ManagerViewModel<Role, RoleEntity>(unitOfWork, mapper, regionManager)
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
