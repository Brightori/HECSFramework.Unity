using System.Runtime.CompilerServices;
using Components;
using Systems;
using Unity.IL2CPP.CompilerServices;
using UnityEngine;

namespace HECSFramework.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public sealed partial class Entity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        partial void UnityPart()
        {
            if (this.TryGetComponent(out ActorProviderComponent actorProviderComponent))
            {
                World.GetSingleSystem<PoolingSystem>().Release(actorProviderComponent.Actor);
            }
        }
    }
}