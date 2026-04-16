namespace SnowyRiver.Domain.Shared.Entities;

public interface IHasId<T>
{
    public T Id { get; set; }
}
