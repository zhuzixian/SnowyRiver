using System.Threading.Tasks;
using EntityFrameworkCore.QueryBuilder.Interfaces;
using EntityFrameworkCore.Repository.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Prism.Navigation;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UsersManagerViewModel(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper,
    IDialogHostService dialog,
    IRegionManager regionManager) : UsersManagerViewModelBase<User, UserEntity>(
        unitOfWorkFactory, mapper, dialog, regionManager)
{
}
