using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using SnowyRiver.WPF.MaterialDesignInPrism.Mvvm;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UsersManagerViewModel(IUnitOfWork unitOfWork, IMapper mapper,
    IRegionManager regionManager) : RegionViewModelBase(regionManager)
{
    private DelegateCommand? _refreshCommand;

    public DelegateCommand? RefreshCommand
        => _refreshCommand ??= new DelegateCommand(async () => await RefreshAsync());

    protected virtual async Task RefreshAsync()
    {
        var repository = unitOfWork.Repository<UserEntity>();
        var query = repository.MultipleResultQuery()
            .OrderByDescending(x => x.CreationTime)
            .Page(0, 100);

        var result = await repository.SearchAsync(query);
        Users = mapper.Map<ObservableCollection<User>>(result);
        for (var i = 0; i < Users.Count; i++)
        {
            Users[i].SortId = i + 1;
        }
    }

    private ObservableCollection<User> _users = [];

    public ObservableCollection<User> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }
}
