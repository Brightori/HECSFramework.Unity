using System;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using Helpers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace HECSFramework.Unity
{
    public static partial class ActorExtentions
    {
        public static bool IsAlive(this IActor actor)
        {
            return actor != null && actor.Entity.IsAlive();
        }

        public static IActor AsActor(this Entity entity)
        {
            return entity.GetComponent<ActorProviderComponent>().Actor;
        }

        public static async ValueTask<IActor> GetActor(this ViewReferenceComponent viewReferenceComponent, Action<IActor> callBack = null)
        {
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync();
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

        public static async ValueTask<IActor> GetActor(this ViewReferenceComponent viewReferenceComponent, Vector3 position, Quaternion rotation, Transform parent)
        {
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync(position, rotation, parent);
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);
            return actorPrfb;
        }

        public static Entity GetEntity(this EntityContainer entityContainer, World world = null, bool needInit = true)
        {
            var entity = new Entity(world, entityContainer.name);
            if (needInit)
                entityContainer.Init(entity);
            entity.GetOrAddComponent<ActorContainerID>().ID = entityContainer.name;
            entity.GenerateGuid();
            return entity;
        }

        public static async ValueTask<IActor> GetActor(this EntityContainer entityContainer, bool needLoadContainer = true, Action<IActor> callBack = null, Vector3 position = default, Quaternion rotation = default, Transform transform = null)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.name;

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID);

            var asynData = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference.AssetGUID);
            var prefab = await asynData.Task;
            var actorPrfb = Object.Instantiate(prefab, position, rotation, transform).GetOrAddMonoComponent<Actor>();

            if (needLoadContainer)
                entityContainer.Init(actorPrfb.Entity);

            Addressables.Release(asynData);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

        public static async ValueTask<IActor> GetActorExluding<Exluding>(this EntityContainer entityContainer, World world = null, bool needLoadContainer = true, Action<IActor> callBack = null, Vector3 position = new Vector3())
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.name;

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID);

            var asynData = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference.AssetGUID);
            var prefab = await asynData.Task;
            var actorPrfb = Object.Instantiate(prefab, position, Quaternion.identity).GetComponent<IActor>();

            actorPrfb.Entity.SetWorld(world);

            if (needLoadContainer)
                entityContainer.Init(actorPrfb.Entity);

            actorPrfb.Entity.RemoveHecsComponentsAndSystems<Exluding>();

            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

#if UNITY_EDITOR
        public static IActor GetActorEditor(this EntityContainer entityContainer, bool needLoadContainer = true, Action<IActor> callBack = null)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + entityContainer.name);

            var prefab = (GameObject)viewReferenceComponent.ViewReference.editorAsset;
            var actorPrfb = Object.Instantiate(prefab).GetComponent<IActor>();

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
            foreach (var c in entity.GetComponentsByType<T>())
            {
                entity.RemoveComponent(TypesMap.GetComponentInfo(c as IComponent).ComponentsMask.Index);
            }

            foreach (var s in entity.Systems.ToArray())
            {
                if (s is T)
                { entity.RemoveHecsSystem(s); }
            }
        }
    }
}
