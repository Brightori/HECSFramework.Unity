using Components;
using HECSFramework.Core;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HECSFramework.Unity
{
    public static partial class ActorExtentions
    {
        

        public static async Task<IActor> GetActor(this ViewReferenceComponent viewReferenceComponent, Action<IActor> callBack = null)
        {
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync();
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);
            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }
        
        public static async Task<IActor> GetActor(this EntityContainer entityContainer, bool needLoadContainer = true,  Action<IActor> callBack = null)
        {
            var unpack = new UnpackContainer(entityContainer);
            var viewReferenceComponent = (unpack.Components.FirstOrDefault(x => x is ViewReferenceComponent)) as ViewReferenceComponent;
            var asynData = viewReferenceComponent.ViewReference.InstantiateAsync();
            var actorPrfb = await asynData.Task;
            Addressables.Release(asynData);

            if (needLoadContainer)
                unpack.InitEntity(actorPrfb);

            callBack?.Invoke(actorPrfb);
            return actorPrfb;
        }
    }
}
