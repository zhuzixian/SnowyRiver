using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UsersManagerViewModel(IUnitOfWork unitOfWork, IMapper mapper,
    IRegionManager regionManager) : ManagerViewModel<User, UserEntity>(
        unitOfWork,
        mapper,
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

    private bool _teamsEnable;
    public bool TeamsEnable
    {
        get => _teamsEnable;
        set => SetProperty(ref _teamsEnable, value);
    }

    protected override string EditorView =>  ViewNames.UserEditorView;
    protected override string ManagerViewRegion => RegionNames.UsersManagerViewRegion;
}
