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
        
        public static System.Numerics.Vector3 AsNumericsV3(this Vector3 source, float height = 0)
            => new System.Numerics.Vector3(source.x, source.y, source.z);
        
        public static System.Numerics.Vector2 AsNumericsV2(this Vector2 source, float height = 0)
            => new System.Numerics.Vector2(source.x, source.y);

        public static Vector3Serialize AsSerializedV3(this Vector3 source, float height = 0)
            => new Vector3Serialize(source.x, source.y, source.z);
        
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