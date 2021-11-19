using System;
using UnityEngine;

namespace Components
{
    public static class Extensions
    {
        public static float Avg(this Vector3 vector3)
            => (Mathf.Abs(vector3.x) + Mathf.Abs(vector3.y) + Mathf.Abs(vector3.z)) / 3;

        public static Vector3 Rotate(this Vector3 point, Quaternion rotation)
            => rotation * point;

        public static Vector3 Normalized(this Vector3 source)
        {
            float num = source.magnitude;
            if (num > 9.99999974737875E-06)
                return source / num;
            else
                throw new ArgumentException();
        }

        public static Vector2 Normalized(this Vector2 source)
        {
            float num = source.magnitude;
            if (num > 9.99999974737875E-06)
                return source / num;
            else
                throw new ArgumentException();
        }

        public static Quaternion Quaternion(this Vector3 point)
            => UnityEngine.Quaternion.Euler(point);

        public static int ToMilliseconds(this float seconds)
            => (int)(seconds * 1000);
    }
}