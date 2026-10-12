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

public static class QueryableExtensions
{
    /// <summary>
    /// Cursor-based pagination
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="cursorPaginationRequest"></param>
    /// <param name="mapper"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSourceEntity"></typeparam>
    /// <typeparam name="TTargetEntity"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidPaginationParameterException"></exception>
    public static async Task<CursorPaginationResponse<TTargetEntity>> PaginateAsync<TSourceEntity, TTargetEntity>(this IQueryable<TSourceEntity> queryable, CursorPaginationRequest cursorPaginationRequest, IMapper mapper, CancellationToken cancellationToken) where TSourceEntity : IEntity<long>
    {
        //Force to sort by id asc 
        IQueryable<TSourceEntity> finalQuery = queryable.OrderBy(x => x.Id);
        IQueryable<TSourceEntity> hasPreviousQuery = queryable.OrderBy(x => x.Id);
        // Only after or before should be used at the same time if they are not null both
        if (cursorPaginationRequest.After != null && cursorPaginationRequest.Before != null)
            throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Only one of before or after should be used at the same time");
        // Validate allowed field to sort and search
        if (cursorPaginationRequest.SortByFields is {Count: > 0})
        {
            foreach (var sortByField in cursorPaginationRequest.SortByFields)
            {
                var field = sortByField.ColName.ToLower();
                var fieldType = typeof(TSourceEntity).GetProperties().FirstOrDefault(x => string.Equals(x.Name, field, StringComparison.CurrentCultureIgnoreCase));
                if (fieldType == null)
                {
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not found");
                }

                var attribute = fieldType.GetCustomAttribute(typeof(SortableAttribute));
                if (attribute == null)
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not sortable");
            }
        }

        if (cursorPaginationRequest.SearchByFields is {Count: > 0})
        {
            foreach (var searchByField in cursorPaginationRequest.SearchByFields)
            {
                var field = searchByField.SearchFieldName.ToLower();
                var fieldType = typeof(TSourceEntity).GetProperties().FirstOrDefault(x => string.Equals(x.Name, field, StringComparison.CurrentCultureIgnoreCase));
                if (fieldType == null)
                {
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not found");
                }

                var attribute = fieldType.GetCustomAttribute(typeof(SearchableAttribute));
                if (attribute == null)
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not searchable");
            }
        }

        // Handle search and order
        if (cursorPaginationRequest.SortByFields is {Count: > 0})
        {
            finalQuery = finalQuery.OrderByFields(fieldsToSort: cursorPaginationRequest.SortByFields);
            hasPreviousQuery = hasPreviousQuery.OrderByFields(fieldsToSort: cursorPaginationRequest.SortByFields);
        }

        if (cursorPaginationRequest.SearchByFields is {Count: > 0})
        {
            finalQuery = finalQuery.SearchByContain(cursorPaginationRequest.SearchByFields);
            hasPreviousQuery = hasPreviousQuery.SearchByContain(cursorPaginationRequest.SearchByFields);
        }

        var hasNext = false;
        var hasPrevious = false;
        // Handle after
        if (cursorPaginationRequest.After != null)
        {
            // Get all items whose ids > after
            finalQuery = finalQuery.Where(i => i.Id > cursorPaginationRequest.After);
            // hasPreviousQuery = hasPreviousQuery.Where(i => i.Id < paginationRequest.After);
            // hasPrevious = await hasPreviousQuery.AnyAsync(cancellationToken);
        }

        // Handler before
        if (cursorPaginationRequest.Before != null)
        {
            // Get all items whose ids > after
            finalQuery = finalQuery.Where(i => i.Id < cursorPaginationRequest.Before).Reverse();
            // hasPreviousQuery = hasPreviousQuery.Where(i => i.Id < paginationRequest.Before);
            // hasPrevious = await hasPreviousQuery.AnyAsync(cancellationToken);
        }


        // Hit to the db to get data back to client side
        var result = await finalQuery
            .ProjectTo<TTargetEntity>(mapper.ConfigurationProvider)
            .Take(cursorPaginationRequest.PageSize + 1)
            .ToListAsync(cancellationToken);
        // Handle first request with all null

        if (cursorPaginationRequest.After == null && cursorPaginationRequest.Before == null)
        {
            hasPrevious = false;
            hasNext = result.Count == cursorPaginationRequest.PageSize + 1;
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
        if (cursorPaginationRequest.After != null)
        {
            hasPrevious = result.Count > 0;
            hasNext = result.Count == cursorPaginationRequest.PageSize + 1;
        }

        if (cursorPaginationRequest.Before != null)
        {
            result.Reverse();
            hasNext = result.Count > 0;
            hasPrevious = result.Count == cursorPaginationRequest.PageSize + 1;
        }

        return new CursorPaginationResponse<TTargetEntity>()
        {
            HasNext = hasNext,
            HasPrevious = hasPrevious,
            Data = result.Take(cursorPaginationRequest.PageSize).ToList()
        };
    }

    /// <summary>
    /// Offset-based pagination
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="offsetPaginationRequest"></param>
    /// <param name="mapper"></param>
    /// <param name="cancellationToken"></param>
    /// <typeparam name="TSourceEntity"></typeparam>
    /// <typeparam name="TTargetEntity"></typeparam>
    /// <returns></returns>
    public static async Task<OffsetPaginationResponse<TTargetEntity>> PaginateAsync<TSourceEntity, TTargetEntity>(this IQueryable<TSourceEntity> queryable, OffsetPaginationRequest offsetPaginationRequest, IMapper mapper, CancellationToken cancellationToken) where TSourceEntity : IEntity<long>
    {
        //Force to sort by id asc 
        IQueryable<TSourceEntity> finalQuery = queryable.OrderBy(x => x.Id);
        // Only after or before should be used at the same time if they are not null both
        if (!offsetPaginationRequest.PageSize.IsValidPageSize() || !offsetPaginationRequest.CurrentPage.IsValidCurrentPage())
            throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Invalid PageSize or CurrentPage");

        // Validate allowed field to sort and search
        if (offsetPaginationRequest.SortByFields is {Count: > 0})
        {
            foreach (var sortByField in offsetPaginationRequest.SortByFields)
            {
                var field = sortByField.ColName.ToLower();
                var fieldType = typeof(TSourceEntity).GetProperties().FirstOrDefault(x => string.Equals(x.Name, field, StringComparison.CurrentCultureIgnoreCase));
                if (fieldType == null)
                {
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not found");
                }

                var attribute = fieldType.GetCustomAttribute(typeof(SortableAttribute));
                if (attribute == null)
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not sortable");
            }
        }

        if (offsetPaginationRequest.SearchByFields is {Count: > 0})
        {
            foreach (var searchByField in offsetPaginationRequest.SearchByFields)
            {
                var field = searchByField.SearchFieldName.ToLower();
                var fieldType = typeof(TSourceEntity).GetProperties().FirstOrDefault(x => string.Equals(x.Name, field, StringComparison.CurrentCultureIgnoreCase));
                if (fieldType == null)
                {
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not found");
                }

                var attribute = fieldType.GetCustomAttribute(typeof(SearchableAttribute));
                if (attribute == null)
                    throw new InvalidPaginationParameterException(nameof(QueryableExtensions), nameof(PaginateAsync), $"Field {field} is not searchable");
            }
        }

        // Handle search and order
        if (offsetPaginationRequest.SortByFields is {Count: > 0})
        {
            finalQuery = finalQuery.OrderByFields(fieldsToSort: offsetPaginationRequest.SortByFields);
        }

        if (offsetPaginationRequest.SearchByFields is {Count: > 0})
        {
            finalQuery = finalQuery.SearchByContain(offsetPaginationRequest.SearchByFields);
        }

        // Count total
        var total = await finalQuery
            .ProjectTo<TTargetEntity>(mapper.ConfigurationProvider)
            .LongCountAsync(cancellationToken);
        // Hit to the db to get data back to client side
        var result = await finalQuery
            .ProjectTo<TTargetEntity>(mapper.ConfigurationProvider)
            .Skip(offsetPaginationRequest.PageSize * (offsetPaginationRequest.CurrentPage - 1))
            .Take(offsetPaginationRequest.PageSize)
            .ToListAsync(cancellationToken);

        return new OffsetPaginationResponse<TTargetEntity>()
        {
            Total = total,
            PageSize = offsetPaginationRequest.PageSize,
            CurrentPage = offsetPaginationRequest.CurrentPage,
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
    public static IQueryable<T> OrderByFields<T>(this IQueryable<T> source, IEnumerable<SortModel> fieldsToSort)
    {
        var expression = source.Expression;
        var count = 0;
        foreach (var item in fieldsToSort)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var memberSelector = Expression.PropertyOrField(parameter, item.ColName);
            var method = string.Equals(item.SortDirection, "desc", StringComparison.OrdinalIgnoreCase) ? (count == 0 ? "OrderByDescending" : "ThenByDescending") : (count == 0 ? "OrderBy" : "ThenBy");
            expression = Expression.Call(typeof(Queryable), method,
                new[] {source.ElementType, memberSelector.Type},
                expression, Expression.Quote(Expression.Lambda(memberSelector, parameter)));
            count++;
        }

        return count > 0 ? source.Provider.CreateQuery<T>(expression) : source;
    }

    /// <summary>
    /// Search by contains and string field by specific field name and its value
    /// </summary>
    /// <param name="source"></param>
    /// <param name="searchModels"></param>
    /// <typeparam name="T">Source Type</typeparam>
    /// <returns></returns>
    public static IQueryable<T> SearchByContain<T>(this IQueryable<T> source, IEnumerable<SearchModel> searchModels)
    {
        var expressions = new List<MethodCallExpression>();
        var parameter = Expression.Parameter(typeof(T), "item");
        foreach (var searchModel in searchModels)
        {
            // Return source if value is null
            if (searchModel.SearchValue == null)
                return source;

            var memberSelector = searchModel.SearchFieldName.Split('.').Aggregate((Expression) parameter, Expression.PropertyOrField);
            var memberType = memberSelector.Type;
            var value = searchModel.SearchValue;
            // Cast value to type of Member
            if (value != null && value.GetType() != memberType)
                value = (string) Convert.ChangeType(value, memberType);
            // Get the method Contains of string type
            var containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
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

            //
            // Add left side and right side --> item.Name.ToLower().Contains("Delete".ToLower())
            var expression = Expression.Call(toLowerLeftSideExp, containsMethod, toLowerRightSideExp);
            expressions.Add(expression);
            var z = Expression.OrElse(expression, expression);
        }

        Expression? combinedExpression = null;
        foreach (var expression in expressions)
        {
            combinedExpression = combinedExpression != null ? Expression.OrElse(combinedExpression, expression) : expression;
        }

        if (combinedExpression != null)
        {
            // Build the predicate  --> item => (item.Name.ToLower().Contains("Delete".ToLower()) OR item.Category.ToLower().Contains("Delete".ToLower()))
            var predicate = Expression.Lambda<Func<T, bool>>(combinedExpression, parameter);
            // Add to the where method
            return source.Where(predicate);
        }

        return source;
    }
}