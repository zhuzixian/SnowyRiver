using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using Mapster;
using MapsterMapper;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.Domain.Entities;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public abstract class ManagerViewModel<TModel, TEntity>(IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager): RegionViewModelBase(regionManager)
    where TEntity: HasNameCreationTimeSoftDeleteEntity<Guid>
    where TModel : EntityModel, new()
{
    public override async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            base.OnNavigatedTo(navigationContext);
            await RefreshAsync();
        }
        catch (Exception e)
        {
            //
        }
    }

    public DelegateCommand UpdateCommand =>
        field ??= new DelegateCommand(async () => await UpdateAsync(),
            () => SelectedModel != default)
            .ObservesProperty(() => SelectedModel);

    private async Task UpdateAsync()
    {
        await NavigateToPermissionEditorViewAsync(SelectedModel!);
    }

    public DelegateCommand CreateCommand
        => field ??= new DelegateCommand(async () => await CreateAsync());

    private async Task CreateAsync()
    {
        await NavigateToPermissionEditorViewAsync(new TModel());
    }

    protected virtual async Task NavigateToPermissionEditorViewAsync(TModel model)
    {
        var parameters = new NavigationParameters
        {
            { "Model", model }
        };
        RegionManager.RequestNavigate(ManagerViewRegion, EditorView, parameters);
        await Task.CompletedTask;
    }

    public DelegateCommand RefreshCommand => 
        field ??= new DelegateCommand(async () => await RefreshAsync());

    private async Task RefreshAsync()
    {
        using var unitOfWork = unitOfWorkFactory.Create();
        var repository = unitOfWork.Repository<TEntity>();
        var query = await GetQueryAsync(repository);
        var result = await repository.SearchAsync(query);
        Models = await mapper.From(result)
            .AdaptToTypeAsync<ObservableCollection<TModel>>();
        for (var i = 0; i < Models.Count; i++)
        {
            Models[i].SortId = i + 1;
        }
    }

    protected virtual Task<IMultipleResultQuery<TEntity>> GetQueryAsync(IRepository<TEntity> repository)
    {
        var query = repository.MultipleResultQuery()
            .OrderByDescending(x => x.CreationTime);
        return Task.FromResult(query);
    }

    public DelegateCommand DeleteCommand => 
        field ??= new DelegateCommand(async () => await DeleteAsync(),
            () => SelectedModel != default)
            .ObservesProperty(() => SelectedModel);

    private async Task DeleteAsync()
    {
        using var unitOfWork = unitOfWorkFactory.Create();
        var repository = unitOfWork.Repository<TEntity>();
        await repository.RemoveAsync(x => x.Id == SelectedModel!.Id);
        await RefreshAsync();
    }


    protected abstract string EditorView { get; }
    protected abstract string ManagerViewRegion { get; }

    public ObservableCollection<TModel> Models
    {
        get;
        set => SetProperty(ref field, value);
    } = [];

    public TModel? SelectedModel
    {
        get;
        set => SetProperty(ref field, value);
    }
}
