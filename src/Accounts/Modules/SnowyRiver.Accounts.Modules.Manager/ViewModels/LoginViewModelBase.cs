using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using System.Threading.Tasks;
using System;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Services;
using SnowyRiver.WPF.Localized;
using System.Linq;
using System.Reactive.Linq;
using Akavache;
using SnowyRiver.Accounts.Modules.Manager.Models;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class LoginViewModel<TUser, TTeam, TRole, TPermission>(
    IAuthenticationService<TUser, TTeam, TRole, TPermission> authenticationService, 
        IRegionManager regionManager) : RegionDialogViewModelBase(regionManager)
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            if (navigationContext.Parameters.TryGetValue<Action>(nameof(NextAction), out var nextAction))
            {
                NextAction = nextAction;
            }

            base.OnNavigatedTo(navigationContext);

            var cacheLogin = await BlobCache.LocalMachine.GetObject<Login>(RememberMeCacheKey)
                .Catch(Observable.Return(default(Login?)));
            if (cacheLogin != null)
            {
                Login.UserName = cacheLogin.UserName;
                Login.Password = cacheLogin.Password;
                RememberMe = true;
            }
        }
        catch (Exception e)
        {
            //
        }
    }

    public Action? NextAction { get; protected set; }

    private DelegateCommand? _confirmCommand;
    public override DelegateCommand ConfirmCommand => _confirmCommand ??= new DelegateCommand(() => _ = ConfirmAsync(),
            () => !string.IsNullOrWhiteSpace(Login.UserName) && !string.IsNullOrWhiteSpace(Login.Password))
        .ObservesProperty(() => Login.UserName)
        .ObservesProperty(() => Login.Password);

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
            var (isLoginSucceed, loginFailedReason) = await authenticationService.LoginAsync(Login.UserName, Login.Password);
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
                if (RememberMe)
                {
                    await BlobCache.LocalMachine.InsertObject(RememberMeCacheKey, Login);
                }
                else
                {
                    await BlobCache.LocalMachine.InvalidateObject<Login>(RememberMeCacheKey);
                }

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

    private Login? _login;
    public Login Login => _login ??= new Login();

    private bool _rememberMe;
    public bool RememberMe
    {
        get => _rememberMe;
        set => SetProperty(ref _rememberMe, value);
    }

    private string _message = string.Empty;

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    protected virtual string RememberMeCacheKey => "Accounts.RememberMe";
}
