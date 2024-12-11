using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Commands;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Models;
using UserEntity = SnowyRiver.Accounts.Domain.Entities.User;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class UsersManagerViewModel(IUnitOfWork unitOfWork, IMapper mapper,
    IRegionManager regionManager) : ManagerViewModel<User, UserEntity>(
        unitOfWork,
        mapper,
    regionManager)
{
    protected override string EditorView =>  ViewNames.UserEditorView;
    protected override string ManagerViewRegion => RegionNames.UsersManagerViewRegion;
}
