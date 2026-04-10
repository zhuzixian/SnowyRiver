using SnowyRiver.Domain.Shared.Entities;

namespace SnowyRiver.Accounts.Domain.Shared.Entities;

public interface INamedAccountAuditedEntity<TUser, TTeam>
    : IAccountAuditedEntity<TUser, TTeam>, IHasName;
