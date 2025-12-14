namespace SnowyRiver.Domain.Shared.Entities;
public interface IPagedResult<T>: IListResult<T>, IHasTotalCount
{
}
