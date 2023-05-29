using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.Visual, "this system spawn view to actor and report when spawn view complete")]
    public sealed class SpawnViewSystem : BaseSystem, IAfterEntityInit 
    {
        [Required]
        public ViewReferenceGameObjectComponent viewReferenceGameObject;

        [Required]
        public UnityTransformComponent unityTransform;
        private GameObject pooling;


        public override void InitSystem()
        {
            
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        public async void AfterEntityInit()
        {
            pooling = await Owner.World.GetSingleSystem<PoolingSystem>().GetViewFromPool(viewReferenceGameObject.ViewReference);

            pooling.transform.position = unityTransform.Transform.position;
            pooling.transform.rotation = unityTransform.Transform.rotation;
            pooling.transform.SetParent(unityTransform.Transform);
            pooling.transform.localPosition = Vector3.zero;

            var injectActor = pooling.GetComponentsInChildren<IHaveActor>();

            foreach (var inject in injectActor)
            {
                inject.Actor = Owner.AsActor();
            }

            AfterViewService.ProcessAfterView(Owner, pooling);
        }
    }

    public static class AfterViewService
    {
        public  static void ProcessAfterView(Entity entity, GameObject view)
        {
            entity.AddComponent(new ViewReadyTagComponent()).View = view;

            var initWithView = entity.GetComponentsByType<IInitAfterView>();

            foreach (var iv in initWithView)
            {
                iv.InitAferView();
            }

            foreach (var s in entity.Systems)
            {
                if (s is IInitAfterView initAferView)
                {
                    initAferView.InitAferView();
                }
            }
        }
    }
}