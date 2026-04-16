using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SnowyRiver.Accounts.Domain.Entities;
using SnowyRiver.Domain.Shared.Entities;
using SnowyRiver.Domain.Shared.Services;

namespace SnowyRiver.Accounts.EntityFramework.Interceptors;

public class AuditInterceptor<TUser, TRole, TTeam, TPermission,
    TUserHistory, TRoleHistory, TTeamHistory, TPermissionHistory>(ICurrentUserServices currentUserServices)
    : EF.Interceptors.AuditInterceptor(currentUserServices)
    where TTeam : Team<TUser, TRole, TTeam, TPermission>
    where TUser : User<TUser, TRole, TTeam, TPermission>
    where TRole : Role<TUser, TRole, TTeam, TPermission>
    where TPermission : Permission<TUser, TRole, TTeam, TPermission>
    where TUserHistory :  AccountEntityHistory<TUser, TUser, TTeam>, new()
    where TTeamHistory :  AccountEntityHistory<TTeam, TUser, TTeam>, new()
    where TRoleHistory :  AccountEntityHistory<TRole, TUser, TTeam>, new()
    where TPermissionHistory :  AccountEntityHistory<TPermission, TUser, TTeam>, new()
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

        EnsureAutoHistory<TUser, Guid, TUserHistory>(context);
        EnsureAutoHistory<TTeam, Guid, TTeamHistory>(context);
        EnsureAutoHistory<TRole, Guid, TRoleHistory>(context);
        EnsureAutoHistory<TPermission, Guid, TPermissionHistory>(context);
    }

    protected virtual void EnsureAutoHistory<TEntity, TEntityId, TEntityHistory>(
        DbContext context)
        where TEntity : class, IEntity<TEntityId>
        where TEntityHistory : class, IEntityHistory<TEntity, TEntityId, TUser, TTeam>, new()
    {
        var entries = context.ChangeTracker.Entries<TEntity>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified
                or EntityState.Deleted)
            .ToList();

        foreach (var history in from entityEntry in entries
                 let entity = entityEntry.Entity
                 let propertyValues = entityEntry.CurrentValues.Clone()
                 let snapshot = (TEntity)propertyValues.ToObject()
                 select new TEntityHistory
                 {
                     EntityId = entity.Id,
                     Action = entityEntry.State.ToString(),
                     SnapShot = snapshot,
                     CreationTime = DateTime.Now,
                     CreatorUserId = CurrentUserServices.UserId,
                     CreatorTeamId = CurrentUserServices.TeamId,
                     LastModifierUserId = CurrentUserServices.UserId,
                     LastModifierTeamId = CurrentUserServices.TeamId,
                     UserId = CurrentUserServices.UserId,
                     TeamId = CurrentUserServices.TeamId
                 })
        {
            context.Set<TEntityHistory>().Add(history);
        }
    }
}
