using System.Collections.ObjectModel;
using System.Threading.Tasks;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class PermissionEditorViewModelBase<
    TUser, TUserEntity,
    TRole, TRoleEntity,
    TTeam, TTeamEntity,
    TPermission, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<TPermission, TPermissionEntity>(unitOfWorkFactory, mapper, regionManager)
    where TUserEntity : SnowyRiver.Accounts.Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUser : User<TUser, TRole, TTeam, TPermission>, new()
    where TRoleEntity : SnowyRiver.Accounts.Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRole : Role<TUser, TRole, TTeam, TPermission>, new()
    where TTeamEntity : SnowyRiver.Accounts.Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>, new()
    where TPermissionEntity : SnowyRiver.Accounts.Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>, new()
{
    public virtual ObservableCollection<string>? PermissionCodes
    {
        get;
        protected set => SetProperty(ref field, value);
    }
}
