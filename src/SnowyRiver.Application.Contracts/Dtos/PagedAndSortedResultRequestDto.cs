namespace Strawberry.Application.Contracts;
public class PagedAndSortedResultRequestDto:PagedResultRequestDto
{
    public virtual string? Sorting { get; set; }
}
