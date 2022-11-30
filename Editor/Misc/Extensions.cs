using System;
using System.Collections.Generic;
using System.Linq;

namespace CatAsset.Editor
{
    public static class Extensions
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }

        public static void Order<T>(this List<T> self, bool ascending) where T : IComparable<T>
        {
            self.Sort(((x, y) =>
            {
                int result = x.CompareTo(y);
                if (!ascending)
                {
                    //倒序排序

                    if (result == -1)
                    {
                        result = 1;

                    }else if (result == 1)
                    {
                        result = -1;
                    }
                }

                return result;
            }));
        }
    }
}
