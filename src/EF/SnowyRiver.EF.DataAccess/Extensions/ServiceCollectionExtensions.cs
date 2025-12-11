using Microsoft.Extensions.DependencyInjection;
using SnowyRiver.EF.DataAccess.Abstractions;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUnitOfWorkFactory(this IServiceCollection services,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        EntityFrameworkCore.UnitOfWork.Extensions.ServiceCollectionExtensions.AddUnitOfWork(services, serviceLifetime);
        services.AddSingleton<IUnitOfWorkFactory, UnitOfWorkFactory>();
        return services;
    }
}
