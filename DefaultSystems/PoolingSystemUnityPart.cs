using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
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

        private readonly Dictionary<string, HECSPool> pools = new(64);
        private Dictionary<int, HECSPool> objectIDToPool = new Dictionary<int, HECSPool>(64);

        private async UniTask<HECSPool> GetPool(AssetReference assetReference)
        {
        getPool:

            if (pools.TryGetValue(assetReference.AssetGUID, out var pool))
                return pool;

            var assetService = EntityManager.Default.GetSingleSystem<AssetService>();
            await assetService.GetAsset<GameObject>(assetReference, isForceRelease: true);
            var container = AssetContainerHolder<GameObject>.AssetReferenceToAssetContainer[assetReference];

            if (pools.ContainsKey(assetReference.AssetGUID))
                goto getPool;

            pools.Add(assetReference.AssetGUID, new HECSPool(container, objectIDToPool, maxPoolSize));
            return pools[assetReference.AssetGUID];
        }

        public async UniTask<T> GetActorFromPool<T>(AssetReference assetReference, World world = null, bool init = true, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null, CancellationToken cancellationToken = default) where T : Actor
        {
            var view = await GetViewFromPool(assetReference);

            if (cancellationToken.IsCancellationRequested)
            {
                ReleaseView(view);
                throw new OperationCanceledException("[Pooling] GetActor canceled");
            }

            var actor = view.GetOrAddMonoComponent<T>();

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

            return actor;
        }

        public async UniTask<T> GetActorFromPool<T>(EntityContainer entityContainer, 
            World world = null, bool init = true, Vector3 position = default, 
            Quaternion rotation = default, Transform parent = null, CancellationToken cancellationToken = default) where T : Actor
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var view = await GetActorFromPool<T>(viewReferenceComponent.ViewReference, world, false, position, rotation, parent, cancellationToken);

            if (init)
            {
                view.InitActorWithoutEntity(world);
                entityContainer.Init(view.Entity);
            }

            return view;
        }

        public async UniTask<GameObject> GetViewFromPool(AssetReference assetReference, Vector3 position = default, Quaternion rotation = default, Transform parent = default, CancellationToken cancellationToken = default)
        {
            var pool = await GetPool(assetReference);
            var view = await pool.Get(position, rotation, parent, cancellationToken);
            view.SetActive(true);

            if (view.TryGetComponent(out IPoolableView poolableView))
                poolableView.Start();

            return view;
        }

        public async UniTask Warmup(AssetReference viewReference, int count, CancellationToken token = default)
        {
            var neededHandler = await GetPool(viewReference);

            for (int i = 0; i < count; i++)
            {
                var go = await  neededHandler.Get(default, default, null, token);
                this.objectIDToPool[go.GetInstanceID()] = neededHandler;
                ReleaseView(viewReference, go).Forget();
            }
        }

        public async UniTask Warmup(EntityContainer entityContainer, int count, CancellationToken token = default)
        {
            if (entityContainer.TryGetComponent(out ViewReferenceComponent view))
               await Warmup(view.ViewReference, count, token); 
            else
                throw new InvalidOperationException("we dont have view ref on this container " + entityContainer);
        }

        public void Release(Actor actor)
        {
            ReleaseView(actor.gameObject);
        }

        public void ReleaseView(IPoolableView poolableView)
        {
            ReleaseView(poolableView.View);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseView(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            if (objectIDToPool.TryGetValue(gameObject.GetInstanceID(), out var poolContainer))
            {
                gameObject.transform.SetParent(null);
                gameObject.SetActive(false);
                poolContainer.Release(gameObject);
            }
            else
            {
                MonoBehaviour.Destroy(gameObject);
            }
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
