using System;
using Commands;
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

            Owner.Command(new ViewReadyCommand());
            Owner.AddHecsComponent(new ViewReadyTagComponent());
        }
    }
}