namespace SnowyRiver.Application.Contracts.Dtos;
public interface IPagedResult<T>: IListResult<T>, IHasTotalCount
{
}
