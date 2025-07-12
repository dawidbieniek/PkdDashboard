using System.Linq.Expressions;

namespace PkdDashboard.WebApp.Services.Data;

public static class IEnumerableExtensions
{
    public static IQueryable<T> SkipTake<T>(this IQueryable<T> source, PagerQuery pagerQuery) where T : class
    {
        return source.Skip(pagerQuery.Page * pagerQuery.PageSize).Take(pagerQuery.PageSize);
    }
    public static IQueryable<T> SkipTake<T>(this IQueryable<T> source, PagerSearchQuery pagerSearchQuery) where T : class 
        => source.SkipTake(pagerSearchQuery.PagerQuery);

    public static IQueryable<T> MatchSearchQuery<T>(this IQueryable<T> source, Expression<Func<T, string>> fieldSelector, PagerSearchQuery pagerSearchQuery) where T : class
    {
        if (!pagerSearchQuery.ShouldSearch)
            return source;

        var parameter = fieldSelector.Parameters[0];
        var property = Expression.Invoke(fieldSelector, parameter);
        var notNull = Expression.NotEqual(property, Expression.Constant(null, typeof(string)));
        var containsMethod = typeof(string).GetMethod(nameof(string.Contains), [typeof(string), typeof(StringComparison)])!;
        var contains = Expression.Call(property,
            containsMethod,
            Expression.Constant(pagerSearchQuery.NormalizedSearchQuery),
            Expression.Constant(StringComparison.CurrentCultureIgnoreCase)
        );
        var body = Expression.AndAlso(notNull, contains);
        var lambda = Expression.Lambda<Func<T, bool>>(body, parameter);

        return source.Where(lambda);
    }
}