using System;
using System.Threading.Tasks;
using Commands;
using Components;
using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Systems
{
    [Serializable][Documentation(Doc.Visual, "this system spawn view to actor and report when spawn view complete")]
    public sealed class SpawnViewSystem : BaseSystem, IAfterEntityInit, IReactCommand<RespawnViewCommand>
    {
        [Required]
        public ViewReferenceGameObjectComponent viewReferenceGameObject;

        [Required]
        public UnityTransformComponent unityTransform;
        private GameObject viewGameObject;
        private PoolingSystem poolingSystem;

        public override void InitSystem() { }

        public async void AfterEntityInit()
        {
            poolingSystem = Owner.World.GetSingleSystem<PoolingSystem>();
            await SpawnView();
        }

        public async void CommandReact(RespawnViewCommand command)
        {
            AfterViewService.ProcessReset(Owner, viewGameObject);
            poolingSystem.ReleaseView(viewGameObject);
            //if we destroy gameObject, we need to wait for the next frame
            await UniTask.Yield();
            await SpawnView();
        }

        private async UniTask SpawnView()
        {
            //after ProcessAfterView component collect all poolable views to release on destroy
            Owner.GetOrAddComponent<PoolableViewsProviderComponent>();
            
            viewGameObject = await poolingSystem.GetViewFromPool(viewReferenceGameObject.ViewReference);
            viewGameObject.transform.position = unityTransform.Transform.position;
            viewGameObject.transform.rotation = unityTransform.Transform.rotation;
            viewGameObject.transform.SetParent(unityTransform.Transform);
            viewGameObject.transform.localPosition = Vector3.zero;
            
            var injectActor = viewGameObject.GetComponentsInChildren<IHaveActor>();
            
            foreach (var inject in injectActor)
            {
                inject.Actor = Owner.AsActor();
            }

            await UniTask.WaitUntil(() => Owner.IsInited);
            AfterViewService.ProcessAfterView(Owner, viewGameObject);
        }

        public override void Dispose()
        {
            if (!EntityManager.Default.TryGetSingleComponent<OnApplicationQuitTagComponent>(out _))
            {
                poolingSystem.ReleaseView(viewGameObject);
            }
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
                iv.InitAfterView();
            }

            foreach (var s in entity.Systems)
            {
                if (s is IInitAfterView initAferView)
                {
                    initAferView.InitAfterView();
                }
            }
        }
        
        public  static void ProcessReset(Entity entity, GameObject view)
        {
            entity.RemoveComponent(new ViewReadyTagComponent());

            var toReset = entity.GetComponentsByType<IInitAfterView>();

            foreach (var iv in toReset)
            {
                iv.Reset();
            }

            foreach (var s in entity.Systems)
            {
                if (s is IInitAfterView initAfterView)
                {
                    initAfterView.Reset();
                }
            }
        }
    }
}