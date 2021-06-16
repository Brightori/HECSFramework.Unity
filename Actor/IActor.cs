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

    public interface IActorDependency
    {
    }

    public interface IHaveActor
    {
        IActor Actor { get; set; }
    }
}