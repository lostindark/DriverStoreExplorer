using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Rapr.Utils
{
    public static class LinqExtensions
    {
        public static IOrderedEnumerable<T> OrderByColumnName<T>(this IEnumerable<T> source, string columnName, bool ascending = true)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return source.OrderBy(a => 1);
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, columnName);
            var propertyType = property.Type;
            var lambda = Expression.Lambda(property, parameter);

            var compiledLambda = lambda.Compile();

            var orderByMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == (ascending ? "OrderBy" : "OrderByDescending") && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IOrderedEnumerable<T>)orderByMethod.Invoke(null, new object[] { source, compiledLambda });
        }

        public static IOrderedEnumerable<T> ThenByColumnName<T>(this IOrderedEnumerable<T> source, string columnName, bool ascending = true)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return source;
            }

            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, columnName);
            var propertyType = property.Type;
            var lambda = Expression.Lambda(property, parameter);

            var compiledLambda = lambda.Compile();

            var thenByMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == (ascending ? "ThenBy" : "ThenByDescending") && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), propertyType);

            return (IOrderedEnumerable<T>)thenByMethod.Invoke(null, new object[] { source, compiledLambda });
        }
    }
}

