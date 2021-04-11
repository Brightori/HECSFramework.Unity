using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public partial interface IActor : IEntity
    {
        bool TryGetComponent<T>(out T component, bool lookInChildsToo = false);
        bool TryGetComponents<T>(out T[] components);
        GameObject GameObject { get; }
    }

    public static class ActorExtentions
    {
        public static void SetActor(this IActor actor, IEntity owner)
        {
            actor = owner as IActor;
        }

    }
}