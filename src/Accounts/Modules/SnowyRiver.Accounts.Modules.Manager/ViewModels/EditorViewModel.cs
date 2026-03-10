using System;
using System.Threading;
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

    public DelegateCommand SaveCommand => field ??= new DelegateCommand(() => _ = SaveAsync());

    protected async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using var unitOfWork = UnitOfWorkFactory.Create();
            if (Model.Id != Guid.Empty)
            {
                await UpdateAsync(unitOfWork, cancellationToken);
            }
            else
            {
                await AddAsync(unitOfWork, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            //
        }
       
        await BackAsync(cancellationToken);
    }

    protected virtual async Task UpdateAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
    }

    protected virtual async Task AddAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken = default)
    {
        var entity = await mapper.From(Model)
            .AdaptToTypeAsync<TEntity>();
        await unitOfWork.Repository<TEntity>()
            .AddAsync(entity, cancellationToken);
    }


    public DelegateCommand BackCommand
        => field ??= new DelegateCommand(() => _ =  BackAsync());

    private async Task BackAsync(CancellationToken cancellationToken = default)
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
