namespace SnowyRiver.Domain.Shared.Entities;
public interface IListResult<T>
{
    /// <summary>
    /// List of items.
    /// </summary>
    IReadOnlyList<T> Items { get; set; }
}
