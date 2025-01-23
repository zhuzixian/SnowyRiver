using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using System.Threading.Tasks;
using System;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Services;
using SnowyRiver.WPF.Localized;
using System.Linq;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class LoginViewModel<TUser, TTeam, TRole, TPermission>(
    IAuthenticationService<TUser, TTeam, TRole, TPermission> authenticationService, 
        IRegionManager regionManager) : RegionDialogViewModelBase(regionManager)
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.TryGetValue<Action>(nameof(NextAction), out var nextAction))
        {
            NextAction = nextAction;
        }

        base.OnNavigatedTo(navigationContext);
    }

    public Action? NextAction { get; protected set; }

    private DelegateCommand? _confirmCommand;
    public override DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(() => _ = ConfirmAsync(),
            () => !string.IsNullOrWhiteSpace(UserName) && !string.IsNullOrWhiteSpace(Password))
        .ObservesProperty(() => UserName)
        .ObservesProperty(() => Password);

    private bool _isLoggingIn;
    public bool IsLoggingIn
    {
        get => _isLoggingIn;
        set => SetProperty(ref _isLoggingIn, value);
    }

    public virtual async Task ConfirmAsync()
    {
        try
        {
            IsLoggingIn = true;
            var (isLoginSucceed, loginFailedReason) = await authenticationService.LoginAsync(UserName, Password);
            if (!isLoginSucceed)
            {
                Message = loginFailedReason == LoginFailedReason.NotFoundUser
                    ? LocalizationProvider.GetLocalizedValueFromCurrentAssembly("LoginNotFoundUser")
                    : LocalizationProvider.GetLocalizedValueFromCurrentAssembly("LoginPasswordVerificationFailed");
                if (authenticationService.User != null && authenticationService.User.Teams.Any())
                {
                    authenticationService.SelectedTeam = authenticationService.User.Teams.First();
                }
            }
            else
            {
                NextAction?.Invoke();
            }
        }
        catch (Exception e)
        {
            Message = e.Message;
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    protected virtual async Task HandleNextAsync()
    {
        await Task.CompletedTask;
    }

    private DelegateCommand? _cancelCommand;
    public override DelegateCommand CancelCommand => _cancelCommand ??= new DelegateCommand(Cancel);

    public override void Cancel()
    {
        Environment.Exit(-1);
    }

    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set
        {
            if (SetProperty(ref _userName, value))
            {
                Message = string.Empty;
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                Message = string.Empty;
            }
        }
    }

    private string _message = string.Empty;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }
}
