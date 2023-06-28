using System.Collections.Generic;
using AssetsManagement.Containers;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    [Documentation(Doc.GameLogic, Doc.HECS, "Global pooling system, contains pooling views, containers, actors")]
    public partial class PoolingSystem : BaseSystem
    {
        public const int minPoolSize = 5;
        public const int maxPoolSize = 512;

        private Dictionary<string, HECSPool<AssetReferenceContainer<AssetReference, GameObject>>> pooledGOs = new(64);
        private Dictionary<string, AssetReferenceContainer<AssetReference, EntityContainer>> pooledContainers = new(16);

        public async UniTask<T> GetActorFromPool<T>(AssetReference assetReference, World world = null, bool init = true) where T : Actor
        {
            var view =  await GetViewFromPool(assetReference);
            var actor = view.GetOrAddMonoComponent<Actor>();
            
            if (actor.Entity != null)
            {
                actor.Entity.Dispose();
                actor.Entity = null;
            }

            if (actor.TryGetComponents(out IHaveActor[] needActors))
                foreach (var need in needActors)
                    need.Actor = actor;

            if (init)
                actor.Init(world);

            return (T)actor;
        }

        public async UniTask<T> GetActorFromPool<T>(EntityContainer entityContainer, World world = null, bool init = true) where T: Actor
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var view =  await GetActorFromPool<T>(viewReferenceComponent.ViewReference, world, false);

            if (init)
            {
                view.InitActorWithoutEntity(world);
                entityContainer.Init(view.Entity);
            }

            return view;
        }

        public async UniTask<GameObject> GetViewFromPool(AssetReference assetReference)
        {
            var assetService = EntityManager.Default.GetSingleSystem<AssetsServiceSystem>();
            var containerTask = assetService.GetContainer<AssetReference, GameObject>(assetReference);
            
            if (pooledGOs.TryGetValue(assetReference.AssetGUID, out var pool))
            {
                var view = await pool.Get();
                view.SetActive(true);

                if (view.TryGetComponent(out IPoolableView poolableView))
                    poolableView.Start();

                return view;
            }
            else
            {
                pooledGOs.Add(assetReference.AssetGUID, new HECSPool<AssetReferenceContainer<AssetReference, GameObject>>(containerTask, maxPoolSize));
                return await pooledGOs[assetReference.AssetGUID].Get();
            }
        }

        public async UniTask<EntityContainer> GetEntityContainerFromPool(AssetReference assetReference)
        {
            var assetGuid = assetReference.AssetGUID;
            if (pooledContainers.TryGetValue(assetGuid, out var container))
            {
                return container.Asset;
            }
            
            var assetService = EntityManager.Default.GetSingleSystem<AssetsServiceSystem>();
            var containerTask = assetService.GetContainer<AssetReference, EntityContainer>(assetReference);
            container = await containerTask;

            //release to decrease ref count
            if (pooledContainers.ContainsKey(assetGuid))
            {
                assetService.ReleaseContainer(container);
            }
            
            pooledContainers.Add(assetGuid, container);
            return pooledContainers[assetGuid].Asset;
        }
       
        
        public async void Warmup(EntityContainer entityContainer, int count)
        {
            if (!entityContainer.TryGetComponent(out ViewReferenceComponent view)) return;
            
            var toRelease = new List<GameObject>(count);
            for (var i = 0; i < count; i++)
            {
                var viewObj = await GetViewFromPool(view.ViewReference);
                toRelease.Add(viewObj);
            }

            for (var i = 0; i < count; i++)
            {
                ReleaseView(toRelease[i]);
            }
            toRelease.Clear();
        }

        public void Release(Actor actor)
        {
            if (actor.TryGetHECSComponent(out PoolableViewsProviderComponent poolableViewsProviderComponent))
            {
                foreach (var pview in poolableViewsProviderComponent.Views)
                {
                    ReleaseView(pview);
                }
            }

            if (actor.TryGetHECSComponent(out ViewReferenceComponent viewReferenceComponent))
            {
                actor.Dispose();
                actor.transform.SetParent(null);
                actor.gameObject.SetActive(false);

                if (pooledGOs.TryGetValue(viewReferenceComponent.ViewReference.AssetGUID, out var pool))
                {
                    pooledGOs[viewReferenceComponent.ViewReference.AssetGUID].Release(actor.gameObject);
                    return;
                }
            }

            MonoBehaviour.Destroy(actor.gameObject);
        }

        public void ReleaseView(IPoolableView poolableView)
        {
            poolableView.Stop();
            
            if (pooledGOs.TryGetValue(poolableView.AssetRef.AssetGUID, out var pool))
            {
                poolableView.View.transform.SetParent(null);
                poolableView.View.SetActive(false);
                pooledGOs[poolableView.AssetRef.AssetGUID].Release(poolableView.View);
            }
            else
            {
                HECSDebug.LogError("View does not have pool");
                MonoBehaviour.Destroy(poolableView.View);
            }
        }

        public void ReleaseView(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IPoolableView poolableView))
                ReleaseView(poolableView);
            else
            {
                HECSDebug.LogError("View is not poolable");
                MonoBehaviour.Destroy(poolableView.View);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var pool in pooledGOs.Values)
            {
                pool.Dispose();
            }

            pooledGOs.Clear();
        }
    }
}
