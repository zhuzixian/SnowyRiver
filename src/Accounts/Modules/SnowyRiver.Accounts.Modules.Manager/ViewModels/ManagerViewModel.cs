using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Commands;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using SnowyRiver.Domain.Entities;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public abstract class ManagerViewModel<TModel, TEntity>(IUnitOfWork unitOfWork, 
    IMapper mapper,
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

    private DelegateCommand? _updateCommand;
    public DelegateCommand UpdateCommand =>
        _updateCommand ??= new DelegateCommand(async () => await UpdateAsync(),
            () => SelectedModel != default)
            .ObservesProperty(() => SelectedModel);

    private async Task UpdateAsync()
    {
        await NavigateToPermissionEditorViewAsync(SelectedModel!);
    }

    private DelegateCommand? _createCommand;

    public DelegateCommand CreateCommand
        => _createCommand ??= new DelegateCommand(async () => await CreateAsync());

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

    private DelegateCommand? _refreshCommand;
    public DelegateCommand RefreshCommand => 
        _refreshCommand ??= new DelegateCommand(async () => await RefreshAsync());

    private async Task RefreshAsync()
    {
        var repository = await GetRepositoryAsync();
        var query = await GetQueryAsync(repository);
        var result = await repository.SearchAsync(query);
        Models = mapper.Map<ObservableCollection<TModel>>(result);
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

    private DelegateCommand? _deleteCommand;
    public DelegateCommand DeleteCommand => 
        _deleteCommand ??= new DelegateCommand(async () => await DeleteAsync(),
            () => SelectedModel != default)
            .ObservesProperty(() => SelectedModel);

    private async Task DeleteAsync()
    {
        var repository = await GetRepositoryAsync();
        await repository.RemoveAsync(x => x.Id == SelectedModel!.Id);
        await RefreshAsync();
    }

    protected Task<IRepository<TEntity>> GetRepositoryAsync()
    {
        return Task.FromResult(unitOfWork.Repository<TEntity>());
    }

    protected abstract string EditorView { get; }
    protected abstract string ManagerViewRegion { get; }

    private ObservableCollection<TModel> _models = [];
    public ObservableCollection<TModel> Models
    {
        get => _models;
        set => SetProperty(ref _models, value);
    }

    private TModel? _selectedModel;
    public TModel? SelectedModel
    {
        get => _selectedModel;
        set => SetProperty(ref _selectedModel, value);
    }
}
