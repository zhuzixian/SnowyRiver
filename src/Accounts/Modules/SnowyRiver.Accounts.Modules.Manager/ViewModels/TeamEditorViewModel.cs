using System.Threading.Tasks;
using AutoMapper;
using EntityFrameworkCore.UnitOfWork.Interfaces;
using Prism.Navigation.Regions;
using SnowyRiver.Accounts.Modules.Manager.Interfaces.Models;
using TeamEntity = SnowyRiver.Accounts.Domain.Entities.Team;

namespace SnowyRiver.Accounts.Modules.Manager.ViewModels;
public class TeamEditorViewModel(
    IUnitOfWork unitOfWork, 
    IMapper mapper,
    IRegionManager regionManager)
    : EditorViewModel<Team, TeamEntity>(unitOfWork, mapper, regionManager)
{
    protected override async Task MapToEntityAsync(Team model, TeamEntity entity)
    {
        await base.MapToEntityAsync(model, entity);
        entity.Name = model.Name;
    }
}
