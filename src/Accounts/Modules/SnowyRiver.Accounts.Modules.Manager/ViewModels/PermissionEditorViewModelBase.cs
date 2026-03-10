using EntityFrameworkCore.UnitOfWork.Interfaces;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

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

    protected override async Task UpdateAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        await unitOfWork.Repository<TPermissionEntity>()
            .UpdateAsync(x => x.Id == Model.Id,
                b =>
                {
                    b.SetProperty(x => x.Code, Model.Code);
                    b.SetProperty(x => x.Name, Model.Name);
                },
                cancellationToken);
    }
}
