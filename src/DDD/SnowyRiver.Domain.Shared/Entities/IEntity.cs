namespace SnowyRiver.Domain.Shared.Entities;

public interface IEntity<T>
{
    public T Id { get; set; }
}
