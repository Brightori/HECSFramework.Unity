using System.Collections.Generic;
using System.Linq;

namespace HECSFramework.Unity.Helpers
{
    public static class CollectionsExtentions
    {
        public static T GetRandomElement<T>(this List<T> items)
            => items[UnityEngine.Random.Range(0, items.Count)];

        public static T GetRandomElement<T>(this T[] items)
            => items[UnityEngine.Random.Range(0, items.Length)];

        public static T GetRandomElement<T>(this IEnumerable<T> items)
            => items.ElementAt(UnityEngine.Random.Range(0, items.Count()));

        public static void AddUniqueElement<T>(this List<T> list, T element)
        {
            if (list.Contains(element)) return;

            list.Add(element);
        }

        public static void RemoveNullValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TValue : class
        {
            bool containsNull = true;
            while (containsNull)
            {
                containsNull = false;
                TKey toDestroy = default;
                foreach (var kvp in dictionary)
                {
                    if (kvp.Value != null) continue;

                    toDestroy = kvp.Key;
                    containsNull = true;
                    break;
                }

                if (containsNull) dictionary.Remove(toDestroy);
            }
        }
    }
}


