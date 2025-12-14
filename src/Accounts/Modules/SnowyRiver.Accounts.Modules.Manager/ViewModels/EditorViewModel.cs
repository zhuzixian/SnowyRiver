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
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class EditorViewModel<TModel, TEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager) 
    : RegionViewModelBase(regionManager)
    where TModel : EntityModel, new()
    where TEntity : Entity<Guid>
{
    protected readonly IUnitOfWorkFactory UnitOfWorkFactory = unitOfWorkFactory;
    protected readonly IMapper Mapper = mapper;

    private IRegionNavigationJournal? _journal;

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        _journal = navigationContext.NavigationService.Journal;
        base.OnNavigatedTo(navigationContext);

        Model = navigationContext.Parameters.TryGetValue<TModel>(nameof(Model), out var model)
            ? model : new TModel();
    }

    public DelegateCommand SaveCommand => field ??= new DelegateCommand(async () => await SaveAsync());

    private async Task SaveAsync()
    {
        using var unitOfWork = UnitOfWorkFactory.Create();
        var repository = unitOfWork.Repository<TEntity>();
        if (Model.Id != Guid.Empty)
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
        await unitOfWork.SaveChangesAsync();
        await BackAsync();
    }

    protected virtual async Task<TEntity> MapToEntityAsync(TModel model)
    {
        return await Mapper.From(model)
            .AdaptToTypeAsync<TEntity>();
    }

    protected virtual async Task MapToEntityAsync(TModel model, TEntity entity)
    {
        if (entity.Id == Guid.Empty)
        {
            entity.Id = model.Id;
        }

        await Task.CompletedTask;
    }

    public DelegateCommand BackCommand
        => field ??= new DelegateCommand(async () => await BackAsync());

    private async Task BackAsync()
    {
        _journal?.GoBack();
        await Task.CompletedTask;
    }

    public TModel Model
    {
        get;
        set => SetProperty(ref field, value);
    } = new();
}
