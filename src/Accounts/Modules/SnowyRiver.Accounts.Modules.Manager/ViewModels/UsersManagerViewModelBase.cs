using System.Threading.Tasks;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UsersManagerViewModelBase<
    TUser, TUserEntity,
    TRole, TRoleEntity,
    TTeam, TTeamEntity,
    TPermission, TPermissionEntity>(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager) : ManagerViewModel<TUser, TUserEntity>(
        unitOfWorkFactory, mapper, dialog, regionManager)
    where TUserEntity : SnowyRiver.Accounts.Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUser : User<TUser, TRole, TTeam, TPermission>, new()
    where TRoleEntity : SnowyRiver.Accounts.Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRole : Role<TUser, TRole, TTeam, TPermission>, new()
    where TTeamEntity : SnowyRiver.Accounts.Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TTeam : Team<TUser, TRole, TTeam, TPermission>, new()
    where TPermissionEntity : SnowyRiver.Accounts.Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>, new()
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<bool>(nameof(TeamsEnable), out var teamsEnable))
        {
            TeamsEnable = teamsEnable;
        }
        base.OnNavigatedTo(navigationContext);
    }

    protected override async Task NavigateToPermissionEditorViewAsync(TUser model)
    {
        var parameters = new NavigationParameters
        {
            { "Model", model },
            { nameof(TeamsEnable),  TeamsEnable },
        };
        RegionManager.RequestNavigate(ManagerViewRegion, EditorView, parameters);
        await Task.CompletedTask;
    }

    protected override Task<IMultipleResultQuery<TUserEntity>> GetQueryAsync(IRepository<TUserEntity> repository)
    {
        var query = repository.MultipleResultQuery()
            .OrderByDescending(x => x.CreationTime)
            .Include(x => x.Include(u => u.Teams))
            .Include(x => x.Include(u => u.Roles)
                .ThenInclude(r => r.Permissions));
        return Task.FromResult(query);
    }

    public bool TeamsEnable
    {
        get;
        set => SetProperty(ref field, value);
    }

    protected override string EditorView =>  ViewNames.UserEditorView;
    protected override string ManagerViewRegion => RegionNames.UsersManagerViewRegion;
}
