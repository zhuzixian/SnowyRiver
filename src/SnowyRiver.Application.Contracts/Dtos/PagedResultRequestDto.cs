using System.ComponentModel.DataAnnotations;

namespace Strawberry.Application.Contracts;
public class PagedResultRequestDto:LimitedResultRequestDto
{
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
}
