using EntityFrameworkCore.UnitOfWork.Interfaces;

namespace SnowyRiver.EF.DataAccess.Abstractions;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
