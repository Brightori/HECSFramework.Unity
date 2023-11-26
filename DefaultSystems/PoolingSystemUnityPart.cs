using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private Dictionary<string, HECSPool<GameObject>> pooledGOs = new Dictionary<string, HECSPool<GameObject>>(64);
        private Dictionary<string, EntityContainer> pooledContainers = new Dictionary<string, EntityContainer>(16);

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
                //todo мы должны туть брать из пула, а не возвращать независимую копию
                pooledGOs.Add(assetReference.AssetGUID, new HECSPool<GameObject>(Addressables.LoadAssetAsync<GameObject>(assetReference).Task, maxPoolSize));
                return await pooledGOs[assetReference.AssetGUID].Get();
            }
        }

        public async UniTask<GameObject> GetFastViewFromPool(AssetReference assetReference)
        {
            if (pooledGOs.TryGetValue(assetReference.AssetGUID, out var pool))
            {
                var view = await pool.Get();
                view.SetActive(true);
                return view;
            }
            else
            {
                //todo мы должны туть брать из пула, а не возвращать независимую копию
                pooledGOs.Add(assetReference.AssetGUID, new HECSPool<GameObject>(Addressables.LoadAssetAsync<GameObject>(assetReference).Task, maxPoolSize));
                return await pooledGOs[assetReference.AssetGUID].Get();
            }
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
                ReleaseView(viewReference, instance);
            }

            await UniTask.NextFrame();
            Addressables.Release(neededHandler);
        }

        public async void Warmup(EntityContainer entityContainer, int count)
        {
            if (entityContainer.TryGetComponent(out ViewReferenceComponent view))
            {
                var neededHandler = Addressables.LoadAssetAsync<GameObject>(view.ViewReference);
                var needed = await neededHandler.Task;

                for (int i = 0; i < count; i++)
                {
                    var instance = MonoBehaviour.Instantiate(needed.gameObject);
                    ReleaseView(view.ViewReference, instance.gameObject);
                }

                await UniTask.NextFrame();
                Addressables.Release(neededHandler);
            }
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
                MonoBehaviour.Destroy(poolableView.View);
            }
        }

        public void ReleaseView(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IPoolableView poolableView))
            {
                poolableView.Stop();

                if (pooledGOs.ContainsKey(poolableView.AddressableKey))
                {
                    pooledGOs[poolableView.AddressableKey].Release(gameObject);
                    poolableView.View.transform.SetParent(null);
                    poolableView.View.SetActive(false);
                    return;
                }
            }
            MonoBehaviour.Destroy(gameObject);
        }

        public void ReleaseView(AssetReference assetReference, GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IPoolableView poolableView))
                poolableView.Stop();

            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            if (pooledGOs.TryGetValue(assetReference.AssetGUID, out var pool))
            {
                pooledGOs[assetReference.AssetGUID].Release(gameObject);
                return;
            }
            else
            {
                pooledGOs.Add(assetReference.AssetGUID, new HECSPool<GameObject>(Addressables.LoadAssetAsync<GameObject>(assetReference).Task));
                pooledGOs[assetReference.AssetGUID].Release(gameObject);
            }
        }

        /// <summary>
        /// we dont check here is poolable or not and just remove to pool
        /// </summary>
        /// <param name="assetReference"></param>
        /// <param name="gameObject"></param>
        public void ReleaseViewFast(AssetReference assetReference, GameObject gameObject)
        {
            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            if (pooledGOs.TryGetValue(assetReference.AssetGUID, out var pool))
            {
                pooledGOs[assetReference.AssetGUID].Release(gameObject);
                return;
            }
            else
            {
                pooledGOs.Add(assetReference.AssetGUID, new HECSPool<GameObject>(Addressables.LoadAssetAsync<GameObject>(assetReference).Task));
                pooledGOs[assetReference.AssetGUID].Release(gameObject);
            }
        }

        public GameObject GetNewInstance(GameObject actor)
        {
            return MonoBehaviour.Instantiate(actor);
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
