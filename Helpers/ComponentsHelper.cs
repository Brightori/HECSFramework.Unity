using System.Runtime.CompilerServices;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Helpers, "here contains extentions for components")]
    public static class ComponentsHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 GetPosition(this Entity entity)
        {
            return entity.GetComponent<UnityTransformComponent>().Transform.position;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion GetRotation(this Entity entity)
        {
            return entity.GetComponent<UnityTransformComponent>().Transform.rotation;
        }
    }
}