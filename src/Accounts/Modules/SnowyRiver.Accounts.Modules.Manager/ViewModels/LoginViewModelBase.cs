using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using System.Threading.Tasks;
using System;
using SnowyRiver.WPF.Localized;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using Akavache;
using SnowyRiver.Accounts.Modules.Manager.Models;
using SnowyRiver.Accounts.Services.Interfaces;

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

            var cacheLogin = await CacheDatabase.LocalMachine.GetObject<Login>(RememberMeCacheKey)
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

    public override DelegateCommand ConfirmCommand => field ??= new DelegateCommand(() => _ = ConfirmAsync(),
            () => !string.IsNullOrWhiteSpace(Login.UserName) && !string.IsNullOrWhiteSpace(Login.Password))
        .ObservesProperty(() => Login.UserName)
        .ObservesProperty(() => Login.Password);

    public virtual bool EnableRememberMe { get; } = true;

    public bool IsLoggingIn
    {
        get;
        set => SetProperty(ref field, value);
    }

    protected override async Task ConfirmAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            IsLoggingIn = true;
            var (isLoginSucceed, loginFailedReason) = await LoginAsync(Login.UserName, Login.Password, cancellationToken);
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
                    await CacheDatabase.LocalMachine.InsertObject(RememberMeCacheKey, Login);
                }
                else
                {
                    await CacheDatabase.LocalMachine.InvalidateObject<Login>(RememberMeCacheKey);
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

    protected virtual async Task<(bool, LoginFailedReason)> LoginAsync(string username, string password, 
        CancellationToken cancellationToken = default)
    {
        return await authenticationService.LoginAsync(username, password, cancellationToken);
    }

    protected virtual async Task HandleNextAsync()
    {
        await Task.CompletedTask;
    }

    protected override async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        Environment.Exit(-1);
    }

    public Login Login => field ??= new Login();

    public bool RememberMe
    {
        get;
        set => SetProperty(ref field, value);
    }

    public string Message
    {
        get;
        set => SetProperty(ref field, value);
    } = string.Empty;

    protected virtual string RememberMeCacheKey => "Accounts.RememberMe";
}
