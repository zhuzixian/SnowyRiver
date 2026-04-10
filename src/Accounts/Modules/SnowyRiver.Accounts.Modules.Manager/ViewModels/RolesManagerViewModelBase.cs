using System.Threading.Tasks;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class RolesManagerViewModelBase<
    TUser, TUserEntity,
    TRole, TRoleEntity,
    TTeam, TTeamEntity,
    TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager) 
    : ManagerViewModel<TRole, TRoleEntity, TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>(unitOfWorkFactory, mapper, dialog, regionManager)
    where TUserEntity : SnowyRiver.Accounts.Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUser : User<TUser, TRole, TTeam, TPermission>, new()
    where TRoleEntity : SnowyRiver.Accounts.Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRole : Role<TUser, TRole, TTeam, TPermission>, new()
    where TTeamEntity : SnowyRiver.Accounts.Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>, new()
    where TPermissionEntity : SnowyRiver.Accounts.Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>, new()
{
    protected override async Task<IMultipleResultQuery<TRoleEntity>> GetQueryAsync(IRepository<TRoleEntity> repository)
    {
        var query = await base.GetQueryAsync(repository);
        query = query.Include(source => source.Include(x => x.Permissions));
        return query;
    }

    protected override string EditorView => ViewNames.RoleEditorView;
    protected override string ManagerViewRegion => RegionNames.RolesManagerViewRegion;
}
