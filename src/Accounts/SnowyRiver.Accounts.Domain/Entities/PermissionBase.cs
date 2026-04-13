using SnowyRiver.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Entities;
public class Permission<TUser, TRole, TTeam, TPermission> 
    : AccountNamedAuditedEntity<TUser, TTeam>, IHasSortId
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    public int SortId { get; set; }
    public string? Code { get; set; }
    public Guid? ParentId { get; set; }
    public string? Alias { get; set; }
    public string? Description { get; set; }

    public TPermission? Parent { get; set; }
    public List<TPermission>? Children { get; set; }
    public List<TRole>? Roles { get; set; }
}
