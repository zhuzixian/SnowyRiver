using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class TeamsManagerViewModel(IUnitOfWork unitOfWork, IMapper mapper, IRegionManager regionManager) 
    : ManagerViewModel<Team, TeamEntity>(unitOfWork, mapper, regionManager)
{
    protected override string EditorView => ViewNames.TeamEditorView;
    protected override string ManagerViewRegion => RegionNames.TeamsManagerViewRegion;
}
