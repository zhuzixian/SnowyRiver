using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using System.Threading.Tasks;
using Mapster;
using MapsterMapper;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.Domain.Entities;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class EditorViewModel<TModel, TEntity,
    TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager) 
    : RegionViewModelBase(regionManager)
    where TTeamEntity : Domain.Entities.Team<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TUserEntity : Domain.Entities.User<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TRoleEntity : Domain.Entities.Role<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TPermissionEntity : Domain.Entities.Permission<TUserEntity, TRoleEntity, TTeamEntity, TPermissionEntity>
    where TModel : EntityModel, new()
    where TEntity : Entity<Guid>
{
    protected readonly IUnitOfWorkFactory UnitOfWorkFactory = unitOfWorkFactory;
    protected IMapper Mapper => mapper;

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

    protected static async Task<IList<T1>> GetEntitiesAsync<T1, T2>(IUnitOfWork unitOfWork,
        ObservableCollection<T2> models,
        CancellationToken cancellationToken = default)
        where T1 : AccountNamedAuditedEntity<TUserEntity, TTeamEntity>
        where T2 : EntityModel
    {
        var repository = unitOfWork.Repository<T1>();
        var selectedIds = models.Where(p => p.IsSelected)
            .Select(p => p.Id)
            .Distinct();
        var entities = await repository.SearchAsync(repository.MultipleResultQuery()
            .AndFilter(x => selectedIds.Contains(x.Id)), cancellationToken);
        return entities;
    }

    protected async Task UpdateAsync<T1, T2>(IUnitOfWork unitOfWork, List<T1> list,
        ObservableCollection<T2> collection,
        CancellationToken cancellationToken = default)
        where T1 : AccountNamedAuditedEntity<TUserEntity, TTeamEntity>
        where T2 : EntityModel
    {
        var entities = await GetEntitiesAsync<T1, T2>(unitOfWork, collection, cancellationToken);
        var existingIds = list.Select(p => p.Id).ToHashSet();
        var newIds = entities.Select(p => p.Id).ToHashSet();

        var toRemoveList = list.Where(p => !newIds.Contains(p.Id)).ToList();
        foreach (var item in toRemoveList)
        {
            list.Remove(item);
        }

        var toAddList = entities.Where(p => !existingIds.Contains(p.Id)).ToList();
        foreach (var item in toAddList)
        {
            list.Add(item);
        }
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
