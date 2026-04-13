using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Domain.Shared.Services;

namespace SnowyRiver.Accounts.EntityFramework.Interceptors;

public class AuditInterceptor<TUser, TRole, TTeam, TPermission>(ICurrentUserServices currentUserServices)
    : EF.Interceptors.AuditInterceptor(currentUserServices)
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
{
    protected override void UpdateAuditFields(DbContext context)
    {
        base.UpdateAuditFields(context);

        var accountAuditedEntries = context.ChangeTracker.Entries<AccountAuditedEntity<TUser, TTeam>>();
        foreach (var entry in accountAuditedEntries)
        {
            entry.Entity.CreatorTeam = null;
            entry.Entity.CreatorUser = null;
            entry.Entity.LastModifierTeam = null;
            entry.Entity.LastModifierUser = null;
        }

        var teamAccountAuditedEntries = context.ChangeTracker.Entries<AccountTeamAuditedEntity<TUser, TTeam>>();
        foreach (var entry in teamAccountAuditedEntries)
        {
            entry.Entity.Team = null;
            entry.Entity.User = null;
        }
    }
}
