using System.Collections.Generic;
using System.Linq;

namespace Nobisoft.Core.Extensions
{
    public static class ObjectExtensions
    {
        public static IQueryable<T> ToQueryable<T>(this T instance)
        {
            return ((IEnumerable<T>) new T[1]
            {
                instance
            }).AsQueryable<T>();
        }

        public static bool IsNullOrDefault<T>(this T instance)
        {
            return EqualityComparer<T>.Default.Equals(instance, default (T));
        }
    }
}