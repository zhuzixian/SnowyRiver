namespace SnowyRiver.Domain.Shared.Entities;

public interface IAuditedEntity<T> : IEntity<T>, ICreationAudited, IModificationAudited
{
}
