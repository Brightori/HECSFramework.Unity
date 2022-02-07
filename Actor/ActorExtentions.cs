using Components;
using HECSFramework.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace HECSFramework.Unity
{
    public static partial class ActorExtentions
    {
        private static HECSMask containerMask = HMasks.GetMask<ActorContainerID>();
        private static HECSMask viewReferenceMask = HMasks.GetMask<ActorContainerID>();

        public static IActor AsActor(this IEntity entity)
        {
            return entity as IActor;
        }

        public static async Task<IActor> GetActor(this ViewReferenceComponent viewReferenceComponent, Action<IActor> callBack = null)
        {
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync();
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }
        
        public static IEntity GetEntity(this EntityContainer entityContainer, int worldIndex = 0)
        {
            var entity = new Entity(entityContainer.name, worldIndex);
            entityContainer.Init(entity);
            entity.GetOrAddComponent<ActorContainerID>().ID = entityContainer.name;
            entity.GenerateGuid();
            return entity;
        }
        
        public static EntityModel GetEntityModel(this EntityContainer entityContainer, int worldIndex = 0)
        {
            var entity = new EntityModel(worldIndex, entityContainer.name);
            entityContainer.Init(entity);
            entity.GetOrAddComponent<ActorContainerID>(containerMask) .ID = entityContainer.name;
            entity.GenerateGuid();
            return entity;
        }

        public static async Task<IActor> GetActor(this EntityContainer entityContainer, bool needLoadContainer = true,  Action<IActor> callBack = null)
        {
            var viewReferenceComponent = entityContainer.GetComponent<ViewReferenceComponent>();
            var actorID = entityContainer.name;

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID);

            var asynData = Addressables.LoadAssetAsync<GameObject>(viewReferenceComponent.ViewReference.AssetGUID);
            var prefab = await asynData.Task;
            var actorPrfb = Object.Instantiate(prefab).GetComponent<IActor>();

            if (needLoadContainer)
                entityContainer.Init(actorPrfb);

            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }

#if UNITY_EDITOR
        public static IActor GetActorEditor(this EntityContainer entityContainer, bool needLoadContainer = true,  Action<IActor> callBack = null)
        {
            var entityModel = new EntityModel(0, entityContainer.name);
            entityContainer.Init(entityModel);
            var unpack = new UnpackContainer(entityContainer);
            var viewReferenceComponent = entityModel.GetHECSComponent<ViewReferenceComponent>(ref viewReferenceMask);
            var actorID = entityModel.GetHECSComponent<ActorContainerID>(ref containerMask);

            if (viewReferenceComponent == null)
                throw new Exception("нет вью рефа у " + actorID.ID);

            var prefab = (GameObject)viewReferenceComponent.ViewReference.editorAsset;
            var actorPrfb = Object.Instantiate(prefab).GetComponent<IActor>();

            if (needLoadContainer)
                entityContainer.Init(actorPrfb);

            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }
#endif
        
        public static T GetComponentInstance<T>(this EntityContainer entityContainer) where T : class, IComponent, new()
        {
            foreach (var componentBluePrint in entityContainer.Components)
            {
                if (!(componentBluePrint is ComponentBluePrintContainer<T>)) continue;

                return (T)componentBluePrint.GetHECSComponent;
            }

            throw new ArgumentOutOfRangeException();
        }
    }
}
