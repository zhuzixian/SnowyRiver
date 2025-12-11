using EntityFrameworkCore.UnitOfWork.Interfaces;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.EF.DataAccess;

public class UnitOfWorkFactory(IServiceProvider serviceProvider) : IUnitOfWorkFactory
{
    public IUnitOfWork Create()
    {
        return new ScopedUnitOfWork(serviceProvider);
    }
}
