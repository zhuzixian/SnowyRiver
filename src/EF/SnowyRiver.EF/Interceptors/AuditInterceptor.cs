using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SnowyRiver.Domain.Shared.Entities;
using SnowyRiver.Domain.Shared.Services;

namespace SnowyRiver.EF.Interceptors;

public class AuditInterceptor(ICurrentUserServices currentUserServices): SaveChangesInterceptor
{
    protected ICurrentUserServices CurrentUserServices => currentUserServices;

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditFields(eventData.Context);
        }

        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditFields(eventData.Context);
        }

        return new ValueTask<InterceptionResult<int>>(result);
    }

    protected virtual void UpdateAuditFields(DbContext context)
    {
        var hasCreationTimeEntries = context.ChangeTracker.Entries<IHasCreationTime>();
        foreach (var entry in hasCreationTimeEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreationTime = DateTime.UtcNow;
            }
        }

        var hasModificationTimeEntries = context.ChangeTracker.Entries<IHasModificationTime>();
        foreach (var entry in hasModificationTimeEntries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModificationTime = DateTime.UtcNow;
            }
        }

        var teamEntries = context.ChangeTracker.Entries<IHasTeamId>();
        foreach (var entry in teamEntries)
        {
            entry.Entity.TeamId = currentUserServices.TeamId;
        }

        var userEntries = context.ChangeTracker.Entries<IHasUserId>();
        foreach (var entry in userEntries)
        {
            entry.Entity.UserId = currentUserServices.UserId;
        }

        var creationAuditedEntries = context.ChangeTracker.Entries<ICreationAudited>();
        foreach (var entry in creationAuditedEntries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatorUserId = currentUserServices.UserId;
                entry.Entity.CreatorTeamId = currentUserServices.TeamId;
            }
        }

        var modificationAuditedEntries = context.ChangeTracker.Entries<IModificationAudited>();
        foreach (var entry in modificationAuditedEntries)
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifierUserId = currentUserServices.UserId;
                entry.Entity.LastModifierTeamId = currentUserServices.UserId;
            }
        }
    }
}
