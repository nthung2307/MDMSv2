using System.Linq.Expressions;
using System.Reflection;
using Application.Common.ValueObjects.CursorBased;
using Application.Common.ValueObjects.OffsetBased;
using Application.Common.ValueObjects.Sorting;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Nobisoft.Core.Domain.Entities;
using VF.Core.Domain.Attributes;

namespace Application.Extensions;

public static class QueryableAndFilterExtensions
{
    /// <summary>
    /// Cursor-based pagination
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="cursorPaginationWithAndRequest"></param>
    /// <param name="mapper"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSourceEntity"></typeparam>
    /// <typeparam name="TTargetEntity"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidPaginationParameterException"></exception>
    public static async Task<CursorPaginationResponse<TTargetEntity>> PaginateWithFilterAsync<TSourceEntity, TTargetEntity>(this IQueryable<TSourceEntity> queryable, CursorPaginationRequest cursorPaginationWithAndRequest, IMapper mapper, CancellationToken cancellationToken) where TSourceEntity : IEntity<long>
    {
        //Force to sort by id asc 
        IQueryable<TSourceEntity> finalQuery = queryable.OrderBy(x => x.Id);
        IQueryable<TSourceEntity> hasPreviousQuery = queryable.OrderBy(x => x.Id);
        // Only after or before should be used at the same time if they are not null both
        if (cursorPaginationWithAndRequest.After != null && cursorPaginationWithAndRequest.Before != null)
            throw new InvalidPaginationParameterException(nameof(QueryableWithAndExtensions), nameof(PaginateWithFilterAsync), $"Only one of before or after should be used at the same time");

        // Handle search and order
        if (cursorPaginationWithAndRequest.SortByFields is { Count: > 0 })
        {
            finalQuery = finalQuery.OrderByFieldsWithFilter(fieldsToSort: cursorPaginationWithAndRequest.SortByFields);
            hasPreviousQuery = hasPreviousQuery.OrderByFieldsWithFilter(fieldsToSort: cursorPaginationWithAndRequest.SortByFields);
        }

        if (cursorPaginationWithAndRequest.SearchByFields is { Count: > 0 })
        {
            foreach (var searchByField in cursorPaginationWithAndRequest.SearchByFields)
            {
                finalQuery = finalQuery.SearchByContainWithFilter(searchByField);
                hasPreviousQuery = hasPreviousQuery.SearchByContainWithFilter(searchByField);
            }
        }

        var hasNext = false;
        var hasPrevious = false;
        // Handle after
        if (cursorPaginationWithAndRequest.After != null)
        {
            // Get all items whose ids > after
            finalQuery = finalQuery.Where(i => i.Id > cursorPaginationWithAndRequest.After);
            // hasPreviousQuery = hasPreviousQuery.Where(i => i.Id < paginationRequest.After);
            // hasPrevious = await hasPreviousQuery.AnyAsync(cancellationToken);
        }

        // Handler before
        if (cursorPaginationWithAndRequest.Before != null)
        {
            // Get all items whose ids > after
            finalQuery = finalQuery.Where(i => i.Id < cursorPaginationWithAndRequest.Before).Reverse();
            // hasPreviousQuery = hasPreviousQuery.Where(i => i.Id < paginationRequest.Before);
            // hasPrevious = await hasPreviousQuery.AnyAsync(cancellationToken);
        }


        // Hit to the db to get data back to client side
        var result = await finalQuery
            .ProjectTo<TTargetEntity>(mapper.ConfigurationProvider)
            .Take(cursorPaginationWithAndRequest.PageSize + 1)
            .ToListAsync(cancellationToken);
        // Handle first request with all null

        if (cursorPaginationWithAndRequest.After == null && cursorPaginationWithAndRequest.Before == null)
        {
            hasPrevious = false;
            hasNext = result.Count == cursorPaginationWithAndRequest.PageSize + 1;
        }

        // Recalculate next, previous
        /*
         * Trong case after,
         *      Nếu result.Count > 0 --> có previous else ko có previous 
         *      Nếu result.Count == paginationRequest.PageSize + 1 --> có next  else ko có next
         * Trong case before
         *      Nếu result.Count > 0 --> có next  else ko có next
         *      Nếu result.Count == paginationRequest.PageSize + 1 --> có previous  else ko có previous
         */
        if (cursorPaginationWithAndRequest.After != null)
        {
            hasPrevious = result.Count > 0;
            hasNext = result.Count == cursorPaginationWithAndRequest.PageSize + 1;
        }

        if (cursorPaginationWithAndRequest.Before != null)
        {
            result.Reverse();
            hasNext = result.Count > 0;
            hasPrevious = result.Count == cursorPaginationWithAndRequest.PageSize + 1;
        }

        return new CursorPaginationResponse<TTargetEntity>()
        {
            HasNext = hasNext,
            HasPrevious = hasPrevious,
            Data = result.Take(cursorPaginationWithAndRequest.PageSize).ToList()
        };
    }

    /// <summary>
    /// Offset-based pagination
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="offsetPaginationWithAndRequest"></param>
    /// <param name="mapper"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSourceEntity"></typeparam>
    /// <typeparam name="TTargetEntity"></typeparam>
    /// <returns></returns>
    public static async Task<OffsetPaginationResponse<TTargetEntity>> PaginateWithFilterAsync<TSourceEntity, TTargetEntity>(this IQueryable<TSourceEntity> queryable, OffsetPaginationRequest offsetPaginationWithAndRequest, IMapper mapper, CancellationToken cancellationToken) where TSourceEntity : IEntity<long>
    {
        //Force to sort by id asc 
        IQueryable<TSourceEntity> finalQuery = queryable.OrderBy(x => x.Id);
        // Only after or before should be used at the same time if they are not null both
        if (!offsetPaginationWithAndRequest.PageSize.IsValidPageSize() || !offsetPaginationWithAndRequest.CurrentPage.IsValidCurrentPage())
            throw new InvalidPaginationParameterException(nameof(QueryableWithAndExtensions), nameof(PaginateWithFilterAsync), $"Invalid PageSize or CurrentPage");

        // Handle search and order
        if (offsetPaginationWithAndRequest.SortByFields is { Count: > 0 })
        {
            finalQuery = finalQuery.OrderByFieldsWithFilter(fieldsToSort: offsetPaginationWithAndRequest.SortByFields);
        }

        if (offsetPaginationWithAndRequest.SearchByFields is { Count: > 0 })
        {
            foreach (var searchByField in offsetPaginationWithAndRequest.SearchByFields)
            {
                finalQuery = finalQuery.SearchByContainWithFilter(searchByField);
            }
        }
        
        // if (offsetPaginationWithAndRequest.FilterByFields is {Count: > 0})
        // {
        //     finalQuery = finalQuery.FilterByContain(offsetPaginationWithAndRequest.FilterByFields);
        // }

        // Count total
        var total = await finalQuery
            .ProjectTo<TTargetEntity>(mapper.ConfigurationProvider)
            .LongCountAsync(cancellationToken);
        // Hit to the db to get data back to client side
        var result = await finalQuery
            .ProjectTo<TTargetEntity>(mapper.ConfigurationProvider)
            .Skip(offsetPaginationWithAndRequest.PageSize * (offsetPaginationWithAndRequest.CurrentPage - 1))
            .Take(offsetPaginationWithAndRequest.PageSize)
            .ToListAsync(cancellationToken);

        return new OffsetPaginationResponse<TTargetEntity>()
        {
            Total = total,
            PageSize = offsetPaginationWithAndRequest.PageSize,
            CurrentPage = offsetPaginationWithAndRequest.CurrentPage,
            Data = result
        };
    }

    /// <summary>
    /// Allow to sort by multiple fields
    /// </summary>
    /// <param name="source"></param>
    /// <param name="fieldsToSort"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IQueryable<T> OrderByFieldsWithFilter<T>(this IQueryable<T> source, IEnumerable<SortModel> fieldsToSort)
    {
        var expression = source.Expression;
        var count = 0;
        foreach (var item in fieldsToSort)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var memberSelector = Expression.PropertyOrField(parameter, item.ColName);
            var method = string.Equals(item.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? (count == 0 ? "OrderByDescending" : "ThenByDescending") : (count == 0 ? "OrderBy" : "ThenBy");
            expression = Expression.Call(typeof(Queryable), method,
                new[] { source.ElementType, memberSelector.Type },
                expression, Expression.Quote(Expression.Lambda(memberSelector, parameter)));
            count++;
        }

        return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
    }

    /// <summary>
    /// Search by contains and string field by specific field name and its value
    /// </summary>
    /// <param name="source"></param>
    /// <param name="searchModel"></param>
    /// <typeparam name="T">Source Type</typeparam>
    /// <returns></returns>
    public static IQueryable<T> SearchByContainWithFilter<T>(this IQueryable<T> source, SearchModel searchModel)
    {
        // Return source if value is null
        if (searchModel.SearchValue == null)
            return source;
        var parameter = Expression.Parameter(typeof(T), "item");
        var memberSelector = searchModel.SearchFieldName.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
        var memberType = memberSelector.Type;
        var value = searchModel.SearchValue;
        // Cast value to type of Member
        if (value != null && value.GetType() != memberType)
            value = (string)Convert.ChangeType(value, memberType);
        // Get the method Contains or Equals of string type
        var propertyInfo = typeof(T).GetProperty(searchModel.SearchFieldName, 
            BindingFlags.Public | 
            BindingFlags.NonPublic | 
            BindingFlags.Instance | 
            BindingFlags.IgnoreCase);
        var attribute = propertyInfo?.GetCustomAttribute<SearchableAttribute>();
        MethodInfo methodInfo;
    
        if (attribute != null && attribute.Type == SearchableAttribute.SearchType.Equals)
        {
            methodInfo = typeof(string).GetMethod("Equals", new[] { typeof(string) });
        }
        else
        {
            methodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        }
    
        if (methodInfo == null)
            return source;
        
        // Convert value into expression contains
        var constantExpressionValue = Expression.Constant(value, typeof(string));

        // To lower case both sides (if you want to keep this functionality)
        var toLowerMethod = typeof(string).GetMethod("ToLower", Type.EmptyTypes);
        if (toLowerMethod == null)
            return source;

        var toLowerLeftSideExp = Expression.Call(memberSelector, toLowerMethod);
        var toLowerRightSideExp = Expression.Call(constantExpressionValue, toLowerMethod);

        // Build the method call expression
        var expression = Expression.Call(toLowerLeftSideExp, methodInfo, toLowerRightSideExp);

        // Build the predicate
        var predicate = Expression.Lambda<Func<T, bool>>(expression, parameter);

        // Add to the where method
        return source.Where(predicate);
    }
    public static IQueryable<T> FilterByContain<T>(this IQueryable<T> source, IEnumerable<FilterModel> filterModels)
    {
        var groupedFilters = filterModels
            .GroupBy(f => f.FilterFieldName)
            .Where(g => g.All(item => item.FilterValue != null));

        var parameter = Expression.Parameter(typeof(T), "item");
        List<Expression> groupExpressions = new List<Expression>();

        foreach (var group in groupedFilters)
        {
            var orExpressions = new List<Expression>();

            foreach (var filterModel in group)
            {
                var memberSelector = filterModel.FilterFieldName.Split('.').Aggregate((Expression)parameter, Expression.PropertyOrField);
                var memberType = memberSelector.Type;
                var value = filterModel.FilterValue;

                // Cast value to type of Member
                if (value != null && value.GetType() != memberType)
                    value = (string)Convert.ChangeType(value, memberType);

                // Get the method Contains of string type
                var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
                if (containsMethod == null)
                    return source;

                // Convert value into expression contains
                var constantExpressionValue = Expression.Constant(value, typeof(string));

                // To lower case both sides
                var toLowerMethod = typeof(string).GetMethod("ToLower", System.Type.EmptyTypes);
                if (toLowerMethod == null)
                    return source;

                var toLowerLeftSideExp = Expression.Call(memberSelector, toLowerMethod);
                var toLowerRightSideExp = Expression.Call(constantExpressionValue, toLowerMethod);

                // Add left side and right side --> item.Name.ToLower().Contains("Delete".ToLower())
                var expression = Expression.Call(toLowerLeftSideExp, containsMethod, toLowerRightSideExp);
                orExpressions.Add(expression);
            }

            if (orExpressions.Any())
            {
                var orCombinedExpression = orExpressions.Aggregate(Expression.OrElse);
                groupExpressions.Add(orCombinedExpression);
            }
        }

        if (groupExpressions.Any())
        {
            // Combine all group expressions with AND
            var andCombinedExpression = groupExpressions.Aggregate(Expression.AndAlso);

            // Build the predicate --> item => ((item.Name.ToLower().Contains("Delete".ToLower()) OR item.Category.ToLower().Contains("Delete".ToLower())) AND ...)
            var predicate = Expression.Lambda<Func<T, bool>>(andCombinedExpression, parameter);

            // Add to the where method
            return source.Where(predicate);
        }

        return source;
    }

}