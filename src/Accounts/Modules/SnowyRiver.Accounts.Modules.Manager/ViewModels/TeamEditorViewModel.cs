using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class TeamEditorViewModel(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : TeamEditorViewModelBase<Team, TeamEntity>(unitOfWorkFactory, mapper, regionManager)
{
}
