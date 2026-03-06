using System.Threading.Tasks;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Services.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class TeamEditorViewModelBase<TTeam, TTeamEntity>(
    IUnitOfWorkFactory unitOfWorkFactory, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<TTeam, TTeamEntity>(unitOfWorkFactory, mapper, regionManager)
    where TTeam: Team,new()
    where TTeamEntity : TeamEntity
{
    protected override async Task MapToEntityAsync(TTeam model, TTeamEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
    }
}
