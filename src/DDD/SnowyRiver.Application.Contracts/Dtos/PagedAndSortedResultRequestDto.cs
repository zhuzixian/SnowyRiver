namespace SnowyRiver.Application.Contracts.Dtos;
public class PagedAndSortedResultRequestDto:PagedResultRequestDto
{
    public virtual string? Sorting { get; set; }
}
