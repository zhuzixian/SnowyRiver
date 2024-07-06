namespace Strawberry.Application.Contracts;
public interface IHasTotalCount
{
    /// <summary>
    /// Total count of Items.
    /// </summary>
    long TotalCount { get; set; }
}
