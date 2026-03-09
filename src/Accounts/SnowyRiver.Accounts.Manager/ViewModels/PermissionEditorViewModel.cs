using System.Collections.ObjectModel;
using MapsterMapper;
using Prism.Navigation.Regions;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.Accounts.Manager.ViewModels;

public class PermissionEditorViewModel(IUnitOfWorkFactory unitOfWorkFactory,
    IMapper mapper,
    IRegionManager regionManager) 
    : SnowyRiver.Accounts.Modules.Manager.ViewModels.PermissionEditorViewModel(unitOfWorkFactory, mapper, regionManager)
{
    public override ObservableCollection<string> PermissionCodes { get; protected set; }
        = 
        [
            "User.Create",
            "Product.Delete"
        ];
}
