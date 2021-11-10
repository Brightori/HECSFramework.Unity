using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Pool;

namespace Systems
{
    public partial class PoolingSystem : BaseSystem
    {
        public const int minPoolSize = 10;
        public const int maxPoolSize = 50;

        private Dictionary<string, ObjectPool<Actor>> pooledActors = new Dictionary<string, ObjectPool<Actor>>(16);

        private HECSMask viewRefMask = HMasks.GetMask<ViewReferenceComponent>();

        public async Task<IActor> GetActorFromPool(ViewReferenceComponent viewReferenceComponent)
        {
            if (pooledActors.TryGetValue(viewReferenceComponent.ViewReference.AssetGUID, out var pool))
            {
                pool.Get(out var actor);
                actor.GameObject.SetActive(true);
                return actor;
            }
            else
            {
                var actor = await Addressables.LoadAssetAsync<Actor>(viewReferenceComponent.ViewReference).Task;
                var newpool = new ObjectPool<Actor>(() => GetNewInstance(actor), defaultCapacity: minPoolSize,  maxSize: maxPoolSize);
                var newActor = GetNewInstance(actor);
                pooledActors.Add(viewReferenceComponent.ViewReference.AssetGUID, newpool);
                return newActor;
            }
        }

        public void Release(Actor actor)
        {
            if (actor.TryGetHecsComponent(viewRefMask, out ViewReferenceComponent viewReferenceComponent))
            {
                pooledActors[viewReferenceComponent.ViewReference.AssetGUID].Release(actor);
                actor.GameObject.SetActive(false);
            }
            else
            {
                MonoBehaviour.Destroy(actor);
            }
        }

        public Actor GetNewInstance(Actor actor)
        {
            return MonoBehaviour.Instantiate(actor);
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var pool in pooledActors.Values)
            {
                pool.Dispose();
            }

            pooledActors.Clear();
        }
    }
}
