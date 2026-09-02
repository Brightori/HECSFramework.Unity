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
    [Documentation(Doc.GameLogic, Doc.HECS, "Global pooling system, contains pooling views, containers, actors. Every asset load, instantiate and destroy goes through AssetService")]
    public partial class PoolingSystem : BaseSystem
    {
        public const int minPoolSize = 5;
        public const int maxPoolSize = 512;

        private readonly Dictionary<string, HECSPool> pools = new(64);
        private readonly Dictionary<int, HECSPool> objectIDToPool = new Dictionary<int, HECSPool>(64);
        private AssetService assetService;

        private AssetService AssetService => assetService ??= Owner.World.GetSingleSystem<AssetService>();

        private async UniTask<HECSPool> GetPool(AssetReference assetReference)
        {
            if (pools.TryGetValue(assetReference.AssetGUID, out var pool))
                return pool;

            await AssetService.GetAsset<GameObject>(assetReference, isForceRelease: true);

            // a concurrent GetPool for the same reference may have won while we awaited
            if (pools.TryGetValue(assetReference.AssetGUID, out pool))
                return pool;

            pool = new HECSPool(AssetService, assetReference, objectIDToPool, maxPoolSize);
            pools.Add(assetReference.AssetGUID, pool);
            return pool;
        }

        public void ReleasePool(AssetReference assetReference)
        {
            if (pools.TryGetValue(assetReference.AssetGUID, out var pool))
            {
                pool.Dispose();
                pools.Remove(assetReference.AssetGUID);
            }
        }

        public async UniTask<T> GetActorFromPool<T>(AssetReference assetReference, World world = null, bool init = true, Vector3 position = default,
            Quaternion rotation = default, Transform parent = null, CancellationToken cancellationToken = default) where T : Actor
        {
            var view = await GetViewFromPool(assetReference, position, rotation, parent, cancellationToken);

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

        public async UniTask<GameObject> GetViewFromPool(AssetReference assetReference, Vector3 position = default, Quaternion rotation = default, Transform parent = default, CancellationToken cancellationToken = default, bool active = true)
        {
            var pool = await GetPool(assetReference);
            var view = await pool.Get(position, rotation, parent, cancellationToken);
            view.SetActive(active);

            if (view.TryGetComponent(out IPoolableView poolableView))
                poolableView.Start();

            return view;
        }

        public async UniTask Warmup(AssetReference viewReference, int count, CancellationToken token = default)
        {
            var pool = await GetPool(viewReference);

            for (int i = 0; i < count; i++)
            {
                var go = await pool.Get(default, Quaternion.identity, null, token);
                go.SetActive(false);
                pool.Release(go);
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

            if (objectIDToPool.TryGetValue(gameObject.GetAdaptedInstanceID(), out var pool))
            {
                StopView(gameObject);
                pool.Release(gameObject);
            }
            else
            {
                AssetService.Release(gameObject);
            }
        }

        public async UniTask ReleaseView(AssetReference assetReference, GameObject gameObject)
        {
            if (gameObject == null)
                return;

            StopView(gameObject);
            var pool = await GetPool(assetReference);
            pool.Release(gameObject);
        }

        /// <summary>
        /// we dont check here is poolable or not and just remove to pool
        /// </summary>
        public async UniTask ReleaseViewFast(AssetReference assetReference, GameObject gameObject)
        {
            if (gameObject == null)
                return;

            gameObject.SetActive(false);
            gameObject.transform.SetParent(null);

            var pool = await GetPool(assetReference);
            pool.Release(gameObject);
        }

        private static void StopView(GameObject gameObject)
        {
            if (gameObject.TryGetComponent(out IPoolableView poolableView))
                poolableView.Stop();

            gameObject.transform.SetParent(null);
            gameObject.SetActive(false);
        }

        public override void Dispose()
        {
            base.Dispose();
            DisposePools();
        }

        public void Clear()
        {
            DisposePools();
            AssetService.UnloadUnusedResources();
        }

        private void DisposePools()
        {
            foreach (var pool in pools.Values)
                pool.Dispose();

            pools.Clear();
            objectIDToPool.Clear();
        }
    }
}