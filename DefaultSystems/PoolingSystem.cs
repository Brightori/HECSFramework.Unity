using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    [Documentation(Doc.GameLogic, Doc.HECS, "Global pooling system, contains pooling views, containers, actors")]
    public partial class PoolingSystem : BaseSystem
    {
        public const int minPoolSize = 5;
        public const int maxPoolSize = 512;

        private Dictionary<string, HECSPool<GameObject>> pooledActors = new Dictionary<string, HECSPool<GameObject>>(16);
        private Dictionary<string, HECSPool<GameObject>> pooledGOs = new Dictionary<string, HECSPool<GameObject>>(16);
        private Dictionary<string, Type> pooledActorsType = new Dictionary<string, Type>(16);
        private Dictionary<string, string> viewToContainer = new Dictionary<string, string>(16);
        private Dictionary<string, EntityContainer> pooledContainers = new Dictionary<string, EntityContainer>(16);

        private HECSMask viewRefMask = HMasks.GetMask<ViewReferenceComponent>();
        private HECSMask ActorContainerIDMask = HMasks.GetMask<ActorContainerID>();

        public async Task<T> GetActorFromPool<T>(AssetReference assetReference, EntityContainer entityContainer = null) where T : Component, IActor
        {
            var key = assetReference.AssetGUID;

            if (pooledGOs.TryGetValue(assetReference.AssetGUID, out var viewPool))
            {
                var actor = await GetActorFromPool<T>(key, viewPool, entityContainer);
                return actor;
            }

            if (pooledActors.TryGetValue(key, out var pool))
            {
                var actor = await GetActorFromPool<T>(key, pool, entityContainer);
                return actor;
            }
            else
            {
                var task = Addressables.LoadAssetAsync<GameObject>(assetReference).Task;
                var objFromRef = await task;
                var newActor = GetNewInstance(objFromRef).GetComponent<T>();

                //we have this scope bcz abother task can be completed before and init pool
                if (pooledActors.ContainsKey(assetReference.AssetGUID))
                {
                    if (entityContainer != null)
                        entityContainer.Init(newActor.Entity);

                    return newActor;
                }

                var newpool = new HECSPool<GameObject>(task, maxPoolSize);

                pooledActorsType.Add(key, newActor.GetType());
                pooledActors.Add(key, newpool);

                if (entityContainer != null)
                {
                    entityContainer.Init(newActor.Entity);
                }

                return newActor;
            }
        }

        private async ValueTask<T> GetActorFromPool<T>(string key, HECSPool<GameObject> pool, EntityContainer entityContainer) where T : Component, IActor
        {
            var view = await pool.Get();
            view.SetActive(true);
            T actor = null;

            if (!view.TryGetComponent<T>(out var actorFromView))
            {
                if (pooledActorsType.TryGetValue(key, out var type))
                {
                    view.AddComponent(pooledActorsType[key]);
                    actor = view.GetComponent<T>();
                }
                else
                {
                    var neededType = typeof(T);
                    view.AddComponent(neededType);
                    actor = view.GetComponent<T>();
                    pooledActorsType.TryAdd(key, neededType);
                }
            }
            else
                actor = actorFromView;

            var startOnPool = view.GetComponentsInChildren<IStartOnPooling>();

            foreach (var s in startOnPool)
            {
                s.Start();
            }

            if (entityContainer != null)
                entityContainer.Init(actor.Entity);

            return actor;
        }

        public async Task<Actor> GetActorFromPool(EntityContainer entityContainer)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var key = viewReferenceComponent.ViewReference.AssetGUID;

            if (pooledGOs.TryGetValue(key, out var goPool))
            {
                return await GetActorFromPool<Actor>(key, goPool, entityContainer);
            }

            if (pooledActors.TryGetValue(key, out var pool))
            {
                return await GetActorFromPool<Actor>(key, pool, entityContainer);
            }
            else
            {
                var task = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference).Task;
                var objFromRef = await task;
                var newActor = GetNewInstance(objFromRef).GetComponent<Actor>();
                entityContainer.Init(newActor.Entity);

                if (pooledActors.ContainsKey(viewReferenceComponent.ViewReference.AssetGUID))
                {
                    return newActor;
                }

                var newpool = new HECSPool<GameObject>(task, maxPoolSize);

                pooledActorsType.Add(key, newActor.GetType());
                pooledActors.Add(viewReferenceComponent.ViewReference.AssetGUID, newpool);
                return newActor;
            }
        }

        public async void Warmup(EntityContainer entityContainer, int count)
        {
            if (entityContainer.TryGetComponent(out ViewReferenceComponent view))
            {
                var needed = await Addressables.LoadAssetAsync<GameObject>(view.ViewReference).Task;

                for (int i = 0; i < count; i++)
                {
                    var instance = MonoBehaviour.Instantiate(needed.gameObject);
                    ReleaseView(view.ViewReference, instance.gameObject);
                }

                needed.gameObject.SetActive(false);
            }
        }

        public async ValueTask<GameObject> GetViewFromPool(AssetReference assetReference)
        {
            var key = assetReference.AssetGUID;

            if (pooledGOs.TryGetValue(key, out var pool))
            {
                var view = await pool.Get();
                view.SetActive(true);

                if (view.TryGetComponent(out IPoolableView poolableView))
                    poolableView.Start();

                return view;
            }
            else
            {
                var task = Addressables.LoadAssetAsync<GameObject>(assetReference).Task;
                var objFromRef = await task;
                var newView = MonoBehaviour.Instantiate(objFromRef);
                return newView;
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

        public void Release(Actor actor)
        {
            actor.TryGetComponents(out IPoolableView[] poolableViews);

            foreach (var pview in poolableViews)
            {
                ReleaseView(pview);
            }

            if (actor.TryGetComponent(out ViewReferenceComponent viewReferenceComponent))
            {
                var key = viewReferenceComponent.ViewReference.AssetGUID;
                var go = actor.gameObject;
                MonoBehaviour.Destroy(actor);
                actor.Dispose();
                go.transform.SetParent(null);
                go.SetActive(false);

                if (pooledGOs.ContainsKey(key))
                {
                    pooledGOs[key].Release(go);
                    return;
                }

                if (pooledActors.ContainsKey(key))
                {
                    pooledActors[viewReferenceComponent.ViewReference.AssetGUID].Release(go);
                    return;
                }
            }

            MonoBehaviour.Destroy(actor.gameObject);
        }

        public void ReleaseView(IPoolableView poolableView)
        {
            poolableView.Stop();

            if (!pooledGOs.ContainsKey(poolableView.AddressableKey))
            {
                var assetref = new AssetReference(poolableView.AddressableKey);
                var pool = new HECSPool<GameObject>(assetref.InstantiateAsync().Task);
                pooledGOs.Add(poolableView.AddressableKey, pool);
            }

            pooledGOs[poolableView.AddressableKey].Release(poolableView.View);
            poolableView.View.transform.SetParent(null);
            poolableView.View.SetActive(false);
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

            if (pooledGOs.ContainsKey(assetReference.AssetGUID))
            {
                pooledGOs[assetReference.AssetGUID].Release(gameObject);
                return;
            }
            else
            {
                pooledGOs.Add(assetReference.AssetGUID, new HECSPool<GameObject>(assetReference.InstantiateAsync().Task));
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

            foreach (var pool in pooledActors.Values)
            {
                pool.Dispose();
            }

            pooledActors.Clear();
        }
    }
}
