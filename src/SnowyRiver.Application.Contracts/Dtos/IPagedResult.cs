namespace Strawberry.Application.Contracts;
public interface IPagedResult<T>: IListResult<T>, IHasTotalCount
{
}
