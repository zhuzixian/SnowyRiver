namespace SnowyRiver.Application.Contracts.Dtos;
public interface IListResult<T>
{
    /// <summary>
    /// List of items.
    /// </summary>
    IReadOnlyList<T> Items { get; set; }
}
