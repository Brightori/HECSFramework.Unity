using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Systems
{
    [Documentation(Doc.GameLogic, "Глобальная система пулинга")]
    public partial class PoolingSystem : BaseSystem
    {
        public const int minPoolSize = 5;
        public const int maxPoolSize = 50;

        private Dictionary<string, HECSPool<GameObject>> pooledActors = new Dictionary<string, HECSPool<GameObject>>(16);
        private Dictionary<string, HECSPool<GameObject>> pooledGOs = new Dictionary<string, HECSPool<GameObject>>(16);
        private Dictionary<string, Type> pooledActorsType = new Dictionary<string, Type>(16);
        private Dictionary<string, string> viewToContainer = new Dictionary<string, string>(16);
        private Dictionary<string, EntityContainer> pooledContainers = new Dictionary<string, EntityContainer>(16);

        private HECSMask viewRefMask = HMasks.GetMask<ViewReferenceComponent>();
        private HECSMask ActorContainerIDMask = HMasks.GetMask<ActorContainerID>();

        public async Task<IActor> GetActorFromPool(ViewReferenceComponent viewReferenceComponent, bool setFromContainer = false)
        {
            var key = viewReferenceComponent.ViewReference.AssetGUID;

            if (pooledActors.TryGetValue(key, out var pool))
            {
                var view = await pool.Get();
                view.SetActive(true);
                IActor actor = null;

                if (!view.TryGetComponent<IActor>(out var actorFromView))
                {
                    view.AddComponent(pooledActorsType[key]);
                    actor = view.GetComponent<IActor>();
                }
                else
                    actor = actorFromView;

                if (setFromContainer)
                {
                    if (viewToContainer.TryGetValue(key, out var containerKey))
                    {
                        pooledContainers[containerKey].Init(actor);
                    }
                }

                return actor;
            }
            else
            {
                var task = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference).Task;
                var objFromRef = await task;
                var newActor = GetNewInstance(objFromRef).GetComponent<Actor>();

                if (pooledActors.ContainsKey(viewReferenceComponent.ViewReference.AssetGUID))
                {
                    if (setFromContainer)
                    {
                        if (viewReferenceComponent.Owner.TryGetHecsComponent(ActorContainerIDMask, out ActorContainerID container))
{
                            var loadedContainer = await GetEntityContainerFromPool(container.ID);
                            loadedContainer.Init(newActor);
                        }
                    }

                    return newActor;
                }

                var newpool = new HECSPool<GameObject>(task, maxPoolSize);

                pooledActorsType.Add(key, newActor.GetType());
                pooledActors.Add(viewReferenceComponent.ViewReference.AssetGUID, newpool);

                if (viewReferenceComponent.Owner.TryGetHecsComponent(ActorContainerIDMask, out ActorContainerID actorContainerID))
                {
                    var awaitContainer = await GetEntityContainerFromPool(actorContainerID.ID);

                    if (!viewToContainer.ContainsKey(key))
                        viewToContainer.Add(key, actorContainerID.ID);

                    if (setFromContainer)
                        awaitContainer.Init(newActor);
                }

                return newActor;
            }
        }

        public async Task<GameObject> GetViewFromPool(AssetReference assetReference)
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
                var objFromRef = await  task;
                var newView = MonoBehaviour.Instantiate(objFromRef);

                if (pooledGOs.ContainsKey(key))
                    return newView;

                var newpool = new HECSPool<GameObject>(task, maxPoolSize);
                pooledGOs.Add(key, newpool);
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

        public void Release(Actor actor)
        {
            actor.TryGetComponents(out IPoolableView[] poolableViews);

            foreach (var pview in poolableViews)
            {
                ReleaseView(pview);
            }

            if (actor.TryGetHecsComponent(viewRefMask, out ViewReferenceComponent viewReferenceComponent))
            {
                if (pooledActors.ContainsKey(viewReferenceComponent.ViewReference.AssetGUID))
                {
                    pooledActors[viewReferenceComponent.ViewReference.AssetGUID].Release(actor.gameObject);
                    actor.GameObject.SetActive(false);
                    MonoBehaviour.Destroy(actor);
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
                pooledGOs[poolableView.AddressableKey].Release(gameObject);
                poolableView.View.transform.SetParent(null);
                poolableView.View.SetActive(false);
            }
            else
            {
                MonoBehaviour.Destroy(gameObject);
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
