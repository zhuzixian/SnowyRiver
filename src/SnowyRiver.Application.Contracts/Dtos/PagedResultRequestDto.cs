namespace SnowyRiver.Application.Contracts.Dtos;
public class PagedResultRequestDto:LimitedResultRequestDto
{
    public virtual int SkipCount { get; set; }
}
