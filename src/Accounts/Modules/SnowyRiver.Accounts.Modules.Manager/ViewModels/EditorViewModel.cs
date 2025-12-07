using System;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.Domain.Entities;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class EditorViewModel<TModel, TEntity>(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    IRegionManager regionManager) 
    : RegionViewModelBase(regionManager)
    where TModel : EntityModel, new()
    where TEntity : Entity<Guid>
{
    protected readonly IUnitOfWork UnitOfWork = unitOfWork;
    protected readonly IMapper Mapper = mapper;

    private IRegionNavigationJournal? _journal;

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _journal = navigationContext.NavigationService.Journal;
        base.OnNavigatedTo(navigationContext);

        Model = navigationContext.Parameters.TryGetValue<TModel>(nameof(Model), out var model)
            ? model : new TModel();
    }

    private DelegateCommand? _saveCommand;
    public DelegateCommand SaveCommand => _saveCommand ??= new DelegateCommand(async () => await SaveAsync());

    private async Task SaveAsync()
    {
        var repository = UnitOfWork.Repository<TEntity>();
        if (Model.Id != default)
        {
            var query = repository.SingleResultQuery()
                .AndFilter(entity => entity.Id == Model.Id);
            var entity = await repository.SingleOrDefaultAsync(query);
            await MapToEntityAsync(Model, entity);
            repository.Update(entity);
        }
        else
        {
            var entity = await MapToEntityAsync(Model);
            await repository.AddAsync(entity);
        }
        await UnitOfWork.SaveChangesAsync();
        await BackAsync();
    }

    protected virtual async Task<TEntity> MapToEntityAsync(TModel model)
    {
        return await Mapper.From(model)
            .AdaptToTypeAsync<TEntity>();
    }

    protected virtual async Task MapToEntityAsync(TModel model, TEntity entity)
    {
        if (entity.Id == default)
        {
            entity.Id = model.Id;
        }

        await Task.CompletedTask;
    }

    private DelegateCommand? _backCommand;
    public DelegateCommand BackCommand
        => _backCommand ??= new DelegateCommand(async () => await BackAsync());

    private async Task BackAsync()
    {
        _journal?.GoBack();
        await Task.CompletedTask;
    }

    private TModel _model = new();
    public TModel Model
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }
}
