using EntityFrameworkCore.UnitOfWork.Interfaces;

namespace SnowyRiver.EF.DataAccess.Extensions;

public static class UnitOfWorkExtensions
{
    public static void AttachStandardNavigations(this IUnitOfWork unitOfWork, object entity,
        params string[] excludeProperties)
    {
        var type = entity.GetType();

        var entities = excludeProperties
            .Select(p => type.GetProperty(p)?.GetValue(entity))
            .Where(e => e != null)
            .ToArray();

        foreach (var nav in entities)
        {
            unitOfWork.DbContext.Attach(nav!);
        }
    }
}
