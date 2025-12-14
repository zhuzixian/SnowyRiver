namespace SnowyRiver.Domain.Shared.Entities;
public interface IHasTotalCount
{
    /// <summary>
    /// Total count of Items.
    /// </summary>
    long TotalCount { get; set; }
}
