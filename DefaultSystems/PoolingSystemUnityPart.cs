using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private readonly Dictionary<string, IHECSPool> pools = new(64);
        private readonly Dictionary<string, UniTask> poolsLoadMap = new();
        private readonly Dictionary<string, EntityContainer> pooledContainers = new(16);

        private bool IsPoolExistsFor(AssetReference assetReference)
        {
            return pools.ContainsKey(assetReference.AssetGUID) || poolsLoadMap.ContainsKey(assetReference.AssetGUID);
        }
        private async UniTask<IHECSPool> GetPool(AssetReference assetReference)
        {
            if (poolsLoadMap.TryGetValue(assetReference.AssetGUID, out var load))
            {
                await load;
            }
            else if (!pools.ContainsKey(assetReference.AssetGUID))
            {
                var loadingTCS = new UniTaskCompletionSource();
                poolsLoadMap[assetReference.AssetGUID] = loadingTCS.Task.Preserve();
                var assetService = EntityManager.Default.GetSingleSystem<AssetsServiceSystem>();
                var container = await assetService.GetContainer<AssetReference, GameObject>(assetReference);
                pools.Add(assetReference.AssetGUID, new HECSPool<IAssetContainer<GameObject>>(container, maxPoolSize));
                poolsLoadMap.Remove(assetReference.AssetGUID);
                loadingTCS.TrySetResult();
            }

            return pools[assetReference.AssetGUID];
        }
        public async UniTask<T> GetActorFromPool<T>(AssetReference assetReference, World world = null, bool init = true) where T : Actor
        {
            var view = await GetViewFromPool(assetReference);
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

        public async UniTask<T> GetActorFromPool<T>(EntityContainer entityContainer, World world = null, bool init = true) where T : Actor
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var view = await GetActorFromPool<T>(viewReferenceComponent.ViewReference, world, false);

            if (init)
            {
                view.InitActorWithoutEntity(world);
                entityContainer.Init(view.Entity);
            }

            return view;
        }

        public async UniTask<GameObject> GetViewFromPool(AssetReference assetReference)
        {
            var pool = await GetPool(assetReference);
            var view = await pool.Get();
            view.SetActive(true);

            if (view.TryGetComponent(out IPoolableView poolableView))
                poolableView.Start();

            return view;
        }

        public async UniTask<GameObject> GetFastViewFromPool(AssetReference assetReference)
        {
            var pool = await GetPool(assetReference);
            var view = await pool.Get();
            view.SetActive(true);
            return view;
        }

        public async Task<EntityContainer> GetEntityContainerFromPool(string key)
        {
            if (pooledContainers.TryGetValue(key, out var container))
            {
                return container;
            }
            else
            {
                var loaded = await Addressables.LoadAssetAsync<EntityContainer>(key).Task;

                if (pooledContainers.ContainsKey(key)) return loaded;

                pooledContainers.Add(key, loaded);
                return loaded;
            }
        }

        public async Task<EntityContainer> GetEntityContainerFromPool(AssetReference assetReference)
        {
            if (pooledContainers.TryGetValue(assetReference.AssetGUID, out var container))
            {
                return container;
            }
            else
            {
                var loaded = await Addressables.LoadAssetAsync<EntityContainer>(assetReference).Task;

                if (pooledContainers.ContainsKey(assetReference.AssetGUID)) return loaded;

                pooledContainers.Add(assetReference.AssetGUID, loaded);
                return loaded;
            }
        }

        public async void Warmup(AssetReference viewReference, int count)
        {
            var neededHandler = Addressables.LoadAssetAsync<GameObject>(viewReference);
            var needed = await neededHandler.Task;

            for (int i = 0; i < count; i++)
            {
                var instance = MonoBehaviour.Instantiate(needed.gameObject);
                ReleaseView(viewReference, instance).Forget();
            }

            await UniTask.NextFrame();
            Addressables.Release(neededHandler);
        }

        public async void Warmup(EntityContainer entityContainer, int count)
        {
            if (entityContainer.TryGetComponent(out ViewReferenceComponent view))
            {
                var needed = await Addressables.LoadAssetAsync<GameObject>(view.ViewReference).Task;

                for (int i = 0; i < count; i++)
                {
                    var instance = MonoBehaviour.Instantiate(needed.gameObject);
                    await ReleaseView(view.ViewReference, instance.gameObject);
                }

                needed.gameObject.SetActive(false);
            }
        }

        public async void Release(Actor actor)
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

                if (IsPoolExistsFor(viewReferenceComponent.ViewReference))
                {
                    var pool = await GetPool(viewReferenceComponent.ViewReference);
                    pool.Release(actor.gameObject);
                    return;
                }
            }

            MonoBehaviour.Destroy(actor.gameObject);
        }

        public async void ReleaseView(IPoolableView poolableView)
        {
            poolableView.Stop();

            if (IsPoolExistsFor(poolableView.AssetRef))
            {
                poolableView.View.transform.SetParent(null);
                poolableView.View.SetActive(false);
                (await GetPool(poolableView.AssetRef)).Release(poolableView.View);
            }
            else
            {
                MonoBehaviour.Destroy(poolableView.View);
            }
        }

        public async void ReleaseView(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IPoolableView poolableView))
            {
                poolableView.Stop();

                if (IsPoolExistsFor(poolableView.AssetRef))
                {
                    (await GetPool(poolableView.AssetRef)).Release(gameObject);
                    poolableView.View.transform.SetParent(null);
                    poolableView.View.SetActive(false);
                    return;
                }
            }
            MonoBehaviour.Destroy(gameObject);
        }

        public async UniTask ReleaseView(AssetReference assetReference, GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IPoolableView poolableView))
                poolableView.Stop();

            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            var pool = await GetPool(assetReference);
            pool.Release(gameObject);
        }

        /// <summary>
        /// we dont check here is poolable or not and just remove to pool
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="gameObject"></param>
        public async UniTask ReleaseViewFast(AssetReference assetReference, GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            var pool = await GetPool(assetReference);
            pool.Release(gameObject);

        }

        public GameObject GetNewInstance(GameObject actor)
        {
            return MonoBehaviour.Instantiate(actor);
        }

        public override void Dispose()
        {
            base.Dispose();

            foreach (var pool in pools.Values)
            {
                pool.Dispose();
            }

            pools.Clear();
        }
    }
}
