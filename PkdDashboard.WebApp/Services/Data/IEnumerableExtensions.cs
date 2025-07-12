using Microsoft.EntityFrameworkCore;

using System.Linq.Expressions;

namespace PkdDashboard.WebApp.Services.Data;

public static class IEnumerableExtensions
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public static IQueryable<T> SkipTake<T>(this IQueryable<T> source, PagerQuery pagerQuery) where T : class
    {
        if (pagerQuery == PagerQuery.All)
            return source;
        return source.Skip(pagerQuery.Page * pagerQuery.PageSize).Take(pagerQuery.PageSize);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    public static IEnumerable<T> SkipTake<T>(this IEnumerable<T> source, PagerQuery pagerQuery) where T : class
    {
        if (pagerQuery == PagerQuery.All)
            return source;
        return source.Skip(pagerQuery.Page * pagerQuery.PageSize).Take(pagerQuery.PageSize);
    }

    public static IQueryable<T> SkipTake<T>(this IQueryable<T> source, PagerSearchQuery pagerSearchQuery) where T : class
        => source.SkipTake(pagerSearchQuery.PagerQuery);

    public static IQueryable<T> MatchSearchQuery<T>(this IQueryable<T> source, Expression<Func<T, string>> fieldSelector, PagerSearchQuery pagerSearchQuery) where T : class
    {
        if (!pagerSearchQuery.ShouldSearch)
            return source;

        // Create a new parameter for the lambda
        var parameter = Expression.Parameter(typeof(T), "x");

        // Replace the parameter in the fieldSelector with the new parameter
        var replacer = new ParameterReplaceVisitor(fieldSelector.Parameters[0], parameter);
        var fieldExpr = replacer.Visit(fieldSelector.Body);

        var notNull = Expression.NotEqual(fieldExpr, Expression.Constant(null, typeof(string)));
        var normalizedField = Expression.Call(fieldExpr, nameof(string.ToLower), Type.EmptyTypes);
        var normalizedQuery = Expression.Constant(pagerSearchQuery.NormalizedSearchQuery);

        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) })!;
        var containsCall = Expression.Call(normalizedField, containsMethod, normalizedQuery);

        var body = Expression.AndAlso(notNull, containsCall);
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

        return source.Where(lambda);
    }

    // Helper class to replace parameters in an expression tree
    class ParameterReplaceVisitor(ParameterExpression oldParameter, ParameterExpression newParameter) : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParameter = oldParameter;
        private readonly ParameterExpression _newParameter = newParameter;

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _oldParameter 
                ? _newParameter 
                : base.VisitParameter(node);
        }
    }
}