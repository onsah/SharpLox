namespace Lox
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    public static class EnumerableExtension
    {
        public static IEnumerable<(T, K)> EnumerableZip<T, K>(this IEnumerable<T> first, IEnumerable<K> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));
            return ZipIterator(first, second); 
        }

        public static IEnumerable<(T, K)> ZipIterator<T, K>(IEnumerable<T> first, IEnumerable<K> second)
        {
            using (var e1 = first.GetEnumerator())
            using (var e2 = second.GetEnumerator())
            {
                while (e1.MoveNext() && e2.MoveNext())
                    yield return (e1.Current, e2.Current);
            }
        }
    }
}