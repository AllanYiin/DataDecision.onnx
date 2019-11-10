using System;
using System.Collections.Generic;
using System.Text;

namespace DataDecision.onnx
{
    public static class numpy
    {

        public static int ArgMax<T, K>(this IEnumerable<T> source,
                                       Func<T, K> map,
                                       IComparer<K> comparer = null)
        {
            if (Object.ReferenceEquals(null, source))
                throw new ArgumentNullException("source");
            else if (Object.ReferenceEquals(null, map))
                throw new ArgumentNullException("map");

            int result = 0;
            K maxKey = default(K);
            Boolean first = true;

            if (null == comparer)
                comparer = Comparer<K>.Default;
            int idx = 0;
            foreach (var item in source)
            {
                K key = map(item);

                if (first || comparer.Compare(key, maxKey) > 0)
                {
                    first = false;
                    maxKey = key;
                    result = idx;
                }

                idx += 1;
            }

            if (!first)
                return result;
            else
                throw new ArgumentException("Can't compute ArgMax on empty sequence.", "source");
        }

    }
}
