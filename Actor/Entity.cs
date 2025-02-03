using System.Runtime.CompilerServices;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Unity;
using Systems;
using Unity.IL2CPP.CompilerServices;

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
                Release(actorProviderComponent.Actor).Forget();
            }
        }

        private async UniTaskVoid Release(Actor actor)
        {
            await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
            World.GetSingleSystem<PoolingSystem>().Release(actor);
        }
    }
}