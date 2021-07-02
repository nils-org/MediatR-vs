using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MediatRvs.Extensions
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> EnsureNotNull<T>(this IEnumerable<T> @this)
        {
            return @this ?? Enumerable.Empty<T>();
        }

        internal static IEnumerable<T> EnsureNotNull<T>(this IEnumerable @this)
        {
            if (@this == null)
            {
                return Enumerable.Empty<T>();
            }

            return @this.Cast<T>();
        }
    }
}
