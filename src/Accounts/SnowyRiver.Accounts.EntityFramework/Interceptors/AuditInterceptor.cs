using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;
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

        var creatorUserEntries = context.ChangeTracker.Entries<IHasCreatorUser<TUser>>();
        foreach (var entry in creatorUserEntries)
        {
            entry.Entity.CreatorUser = null;
        }

        var creatorTeamEntries = context.ChangeTracker.Entries<IHasCreatorTeam<TTeam>>();
        foreach (var entry in creatorTeamEntries)
        {
            entry.Entity.CreatorTeam = null;
        }

        var lastModifierUserEntries = context.ChangeTracker.Entries<IHasLastModifierUser<TUser>>();
        foreach (var entry in lastModifierUserEntries)
        {
            entry.Entity.LastModifierUser = null;
        }

        var lastModifierTeamEntries = context.ChangeTracker.Entries<IHasLastModifierTeam<TTeam>>();
        foreach (var entry in lastModifierTeamEntries)
        {
            entry.Entity.LastModifierTeam = null;
        }

        var teamEntries = context.ChangeTracker.Entries<IHasTeam<TTeam>>();
        foreach (var entry in teamEntries)
        {
            entry.Entity.Team = null;
        }

        var userEntries = context.ChangeTracker.Entries<IHasUser<TUser>>();
        foreach (var entry in userEntries)
        {
            entry.Entity.User = null;
        }
    }
}
