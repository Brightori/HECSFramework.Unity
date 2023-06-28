using System;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using Helpers;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        public static async ValueTask<Actor> GetActor(this ViewReferenceComponent viewReferenceComponent, Action<Actor> callBack = null)
        {
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync();
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

        public static async ValueTask<Actor> GetActor(this ViewReferenceComponent viewReferenceComponent, Vector3 position, Quaternion rotation, Transform parent)
        {
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync(position, rotation, parent);
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);
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

        public static async ValueTask<Actor> GetActor(this EntityContainer entityContainer, World world = null, bool needLoadContainer = true, Action<Actor> callBack = null, Vector3 position = default, Quaternion rotation = default, Transform transform = null)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.CachedName;

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID);

            var asynData = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference.AssetGUID);
            var prefab = await asynData.Task;
            var actorPrfb = Object.Instantiate(prefab, position, rotation, transform).GetOrAddMonoComponent<Actor>();

            if (needLoadContainer)
            {
                actorPrfb.Init(world, initEntity: false);
                entityContainer.Init(actorPrfb.Entity);
            }

            //Addressables.Release(asynData);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

        public static async ValueTask<Actor> GetActorExluding<Exluding>(this EntityContainer entityContainer, World world = null, bool needLoadContainer = true, bool initEntity = true, Action<Actor> callBack = null, Vector3 position = new Vector3())
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.CachedName;

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID);

            var asynData = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference.AssetGUID);
            var prefab = await asynData.Task;
            var actorPrfb = Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<Actor>();

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
