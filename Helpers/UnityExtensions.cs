using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity.Helpers
{
    public static class UnityExtensions
    {
        public static Vector2 AsV2(this Vector3 source)
            => new Vector2(source.x, source.z);

        public static Vector3 AsV3(this Vector2 source, float height = 0)
            => new Vector3(source.x, height, source.y);
        
        public static void RemoveDestroyedValues<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
            where TValue : Object
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