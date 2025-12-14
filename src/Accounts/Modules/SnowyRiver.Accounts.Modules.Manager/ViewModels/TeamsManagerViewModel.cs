using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using SnowyRiver.WPF.MaterialDesignInPrism.Core.Dialogs;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class TeamsManagerViewModel(IUnitOfWorkFactory unitOfWorkFactory, IMapper mapper, 
    IDialogHostService dialog,
    IRegionManager regionManager) 
    : ManagerViewModel<Team, TeamEntity>(unitOfWorkFactory, mapper, dialog, regionManager)
{
    protected override string EditorView => ViewNames.TeamEditorView;
    protected override string ManagerViewRegion => RegionNames.TeamsManagerViewRegion;
}
