using System;
using System.Threading;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using Helpers;
using Systems;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HECSFramework.Unity
{
    public static partial class ActorExtentions
    {
        public static bool IsAlive(this Actor actor)
        {
            return actor != null && actor.Entity.IsAlive();
        }

        public static Actor AsActor(this Entity entity)
        {
            return entity.GetComponent<ActorProviderComponent>().Actor;
        }

        public static async UniTask<Actor> GetActor(this ViewReferenceComponent viewReferenceComponent,
            Vector3 position = default, Quaternion rotation = default, Transform parent = null, CancellationToken cancellationToken = default)
        {
            var assetsService = EntityManager.Default.GetSingleSystem<AssetService>();
            var actorPrfb = await assetsService.GetAssetInstance(viewReferenceComponent.ViewReference, position, rotation, parent, cancellationToken: cancellationToken);
            return actorPrfb.GetComponent<Actor>();
        }

        public static async UniTask<Actor> GetActor(this ViewReferenceComponent viewReferenceComponent, Action<Actor> callBack = null)
        {
            var actorPrfb = await GetActor(viewReferenceComponent, Vector3.zero, Quaternion.identity, null);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

        public static Entity GetEntity(this EntityContainer entityContainer, World world = null, bool needInit = true)
        {
            if (world == null)
                world = EntityManager.Default;

            var entity = world.GetEntityFromPool(entityContainer.CachedName);
            
            if (needInit)
                entityContainer.Init(entity);
            
            entity.GetOrAddComponent<ActorContainerID>().ID = entityContainer.CachedName;
            entity.GenerateGuid();
            return entity;
        }

        /// <summary>
        /// default method to create actor from container
        /// </summary>
        /// <param name="entityContainer"></param>
        /// <param name="world"></param>
        /// <param name="needLoadContainer">by default this true, this is default scenario for init actor with data from operated container, if u want custom scenarios, u should make it false and proceed to custom scenario for initing actor (with container on actor for example)</param>
        /// <param name="callBack"></param>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <param name="transform"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async UniTask<Actor> GetActor(this EntityContainer entityContainer, World world = null, bool needLoadContainer = true, Action<Actor> callBack = null, Vector3 position = default, Quaternion rotation = default, Transform transform = null)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.CachedName;

            if (viewReferenceComponent == null)
                throw new Exception($"actor {actorID} does not have any view ");

            var actorPrfb = await GetActor(viewReferenceComponent, position, rotation, transform);
            if (needLoadContainer)
            {
                actorPrfb.Init(world, initEntity: false);
                entityContainer.Init(actorPrfb.Entity);
            }
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

        public static async UniTask<T> GetActorFromPool<T>(this EntityContainer entityContainer, 
            World world = null, bool needLoadContainer = true, Action<Actor> callBack = null, 
            Vector3 position = default, Quaternion rotation = default, Transform parent = null, 
            CancellationToken cancellationToken = default) where T: Actor
        {
            if (world == null)
                world = EntityManager.Default;

            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.CachedName;

            if (viewReferenceComponent == null)
                throw new Exception($"[Pooling] container {actorID} doesnt have viewrefence");

            var actorPrfb = await world.GetSingleSystem<PoolingSystem>().GetViewFromPool(viewReferenceComponent.ViewReference, position, rotation, parent, cancellationToken);
            var needed = actorPrfb.GetComponent<T>();

            needed.InitActorWithoutEntity(world);
            entityContainer.Init(needed.Entity);

            callBack?.Invoke(needed);
            return needed;
        }

        public static async UniTask<Actor> GetActorExluding<Exluding>(this EntityContainer entityContainer, World world = null, bool needLoadContainer = true, bool initEntity = true, Action<Actor> callBack = null, Vector3 position = new Vector3())
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.CachedName;

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID);


            var actorPrfb = await GetActor(viewReferenceComponent, position);
            actorPrfb.Init(world, false);

            if (needLoadContainer)
                entityContainer.Init(actorPrfb.Entity);

            actorPrfb.Entity.RemoveHecsComponentsAndSystems<Exluding>();
            if(initEntity)
                actorPrfb.Entity.Init();

            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

#if UNITY_EDITOR
        public static Actor GetActorEditor(this EntityContainer entityContainer, bool needLoadContainer = true, Action<Actor> callBack = null)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + entityContainer.name);

            var prefab = (GameObject)viewReferenceComponent.ViewReference.editorAsset;
            var actorPrfb = Object.Instantiate(prefab).GetComponent<Actor>();

            actorPrfb.Init(null, false, false);

            if (needLoadContainer)
                entityContainer.Init(actorPrfb.Entity);

            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }
#endif

        public static T GetComponentInstance<T>(this EntityContainer entityContainer) where T : class, IComponent, new()
        {
            foreach (var componentBluePrint in entityContainer.Components)
            {
                if (!(componentBluePrint.GetHECSComponent is T)) continue;

                return (T)componentBluePrint.GetComponentInstance();
            }

            throw new ArgumentOutOfRangeException();
        }

        public static void RemoveHecsComponentsAndSystems<T>(this Entity entity)
        {
            var hecsPoolArray = HECSPooledArray<T>.GetArray(entity.Components.Count);

            foreach (var c in entity.GetComponentsByType<T>())
            {
                hecsPoolArray.Add(c);
            }

            for (int i = 0; i < hecsPoolArray.Count; i++)
            {
                entity.RemoveComponent(hecsPoolArray.Items[i] as IComponent);
            }

            hecsPoolArray.Release();


            var hecsSystemsPooledArray = HECSPooledArray<ISystem>.GetArray(entity.Systems.Count);

            foreach (var s in entity.Systems)
            {
                if (s is T)
                {
                    hecsSystemsPooledArray.Add(s);
                }
            }

            for (int i = 0; i < hecsSystemsPooledArray.Count; i++)
            {
                entity.RemoveHecsSystem(hecsSystemsPooledArray.Items[i]);
            }

            hecsSystemsPooledArray.Release();
        }
    }
}
