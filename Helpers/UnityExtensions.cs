using System.Collections.Generic;
using Components;
using HECSFramework.Unity;
using UnityEngine;

namespace Helpers
{
    public static class UnityExtensions
    {
        public static Vector2 AsV2(this Vector3 source)
            => new Vector2(source.x, source.z);

        public static Vector3 AsV3(this Vector2 source, float height = 0)
            => new Vector3(source.x, height, source.y);
        
        public static bool TryGetActorFromCollision(this Component component, out Actor actor)
        {
            actor = null;

            if (component.TryGetComponent(out Actor needed))
            {
                actor = needed;
                return true;
            }

            if (component.TryGetComponent(out CollisionProvider collisionProvider))
            {
                actor = collisionProvider.Actor;
                return true;
            }

            return false;
        }

        public static T GetOrAddMonoComponent<T>(this GameObject gameObject) where T: Component
        {
            if (gameObject == null)
                throw new System.Exception("Monobehaviour is null");

            if (gameObject.TryGetComponent(out T component))
            {
                return component;
            }

            return gameObject.AddComponent<T>();
        }

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

        public static Vector3 GetRandomPositionInBounds(Bounds bounds)
        {
            var left = bounds.center.x - bounds.extents.x;
            var right = bounds.center.x + bounds.extents.x;
            var top = bounds.center.y + bounds.extents.y;
            var bottom = bounds.center.y - bounds.extents.y;

            var horizontalRandom = UnityEngine.Random.Range(left, right);
            var verticalRandom = UnityEngine.Random.Range(bottom, top);

            return new Vector3(horizontalRandom, verticalRandom, 0);
        }
    }
}