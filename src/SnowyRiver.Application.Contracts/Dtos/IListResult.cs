namespace Strawberry.Application.Contracts;
public interface IListResult<T>
{
    /// <summary>
    /// List of items.
    /// </summary>
    IReadOnlyList<T> Items { get; set; }
}
