using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rapr.Utils
{
    public static class LinqExtensions
    {
        private static readonly MethodInfo ToSortableStringMethod =
            typeof(LinqExtensions).GetMethod(nameof(ToSortableString), BindingFlags.NonPublic | BindingFlags.Static);

        public static IOrderedEnumerable<T> OrderByColumnName<T>(this IEnumerable<T> source, string columnName, bool ascending = true)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return source.OrderBy(a => 1);
            }

            var lambda = CreateKeySelector<T>(columnName);
            var compiledLambda = lambda.Compile();

            var orderByMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == (ascending ? "OrderBy" : "OrderByDescending") && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), lambda.ReturnType);

            return (IOrderedEnumerable<T>)orderByMethod.Invoke(null, new object[] { source, compiledLambda });
        }

        public static IOrderedEnumerable<T> ThenByColumnName<T>(this IOrderedEnumerable<T> source, string columnName, bool ascending = true)
        {
            if (string.IsNullOrEmpty(columnName))
            {
                return source;
            }

            var lambda = CreateKeySelector<T>(columnName);
            var compiledLambda = lambda.Compile();

            var thenByMethod = typeof(Enumerable)
                .GetMethods()
                .First(m => m.Name == (ascending ? "ThenBy" : "ThenByDescending") && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(T), lambda.ReturnType);

            return (IOrderedEnumerable<T>)thenByMethod.Invoke(null, new object[] { source, compiledLambda });
        }

        private static LambdaExpression CreateKeySelector<T>(string columnName)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            Expression key = Expression.Property(parameter, columnName);

            // Comparer<T>.Default throws for keys that don't implement IComparable (e.g. List<string>),
            // so sort those by their text, the same way ObjectListView does.
            if (!IsComparable(key.Type))
            {
                key = Expression.Call(ToSortableStringMethod, Expression.Convert(key, typeof(object)));
            }

            return Expression.Lambda(key, parameter);
        }

        private static bool IsComparable(Type type)
        {
            return typeof(IComparable).IsAssignableFrom(Nullable.GetUnderlyingType(type) ?? type);
        }

        private static string ToSortableString(object value)
        {
            if (value is string || !(value is IEnumerable items))
            {
                return value?.ToString();
            }

            return string.Join(", ", items.Cast<object>());
        }
    }
}
