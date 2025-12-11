using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

namespace SnowyRiver.Domain.Shared.Extensions;
public static class SetPropertyCallsExtensions
{
    public static Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> Append<TEntity>(
        this Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> first,
        Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> second)
    {
        var replacedExpression = new ReplacingExpressionVisitor(second.Parameters, new[] { first.Body })
            .Visit(second.Body);

        var combinedLambda = Expression.Lambda<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>>(
            replacedExpression,
            first.Parameters
        );

        return combinedLambda;
    }
}
