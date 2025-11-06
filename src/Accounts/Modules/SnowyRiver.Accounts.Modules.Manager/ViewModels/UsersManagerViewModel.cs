using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UsersManagerViewModel(IUnitOfWork unitOfWork, IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager) : ManagerViewModel<User, UserEntity>(
        unitOfWork,
        mapper,
        dialog,
    regionManager)
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<bool>(nameof(TeamsEnable), out var teamsEnable))
        {
            TeamsEnable = teamsEnable;
        }
        base.OnNavigatedTo(navigationContext);
    }

    protected override async Task NavigateToPermissionEditorViewAsync(User model)
    {
        var parameters = new NavigationParameters
        {
            { "Model", model },
            { nameof(TeamsEnable),  TeamsEnable },
        };
        RegionManager.RequestNavigate(ManagerViewRegion, EditorView, parameters);
        await Task.CompletedTask;
    }

    protected override Task<IMultipleResultQuery<UserEntity>> GetQueryAsync(IRepository<UserEntity> repository)
    {
        var query = repository.MultipleResultQuery()
            .OrderByDescending(x => x.CreationTime)
            .Include(x => x.Include(u => u.Teams))
            .Include(x => x.Include(u => u.Roles)
                .ThenInclude(r => r.Permissions));
        return Task.FromResult(query);
    }

    private bool _teamsEnable;
    public bool TeamsEnable
    {
        get => _teamsEnable;
        set => SetProperty(ref _teamsEnable, value);
    }

    protected override string EditorView =>  ViewNames.UserEditorView;
    protected override string ManagerViewRegion => RegionNames.UsersManagerViewRegion;
}
