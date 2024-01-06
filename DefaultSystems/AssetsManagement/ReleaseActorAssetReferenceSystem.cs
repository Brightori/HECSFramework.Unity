using System;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.GameLogic, "listen to disposing actor and release by assetServiceSystem and pooling system")]
    public sealed class ReleaseActorAssetReferenceSystem : BaseSystem, IReactGlobalCommand<ActorViewDisposedCommand>, IGlobalStart
    {
        private AssetsServiceSystem assetService;

        public override void InitSystem()
        {
        }

        public void GlobalStart()
        {
            assetService = EntityManager.Default.GetSingleSystem<AssetsServiceSystem>();
        }

        public async void CommandGlobalReact(ActorViewDisposedCommand command)
        {
            var container =
                await assetService.GetContainer<ActorViewReference, GameObject>(command.Actor
                    .GetHECSComponent<ViewReferenceComponent>().ViewReference);
            container.ReleaseInstance(command.Actor.gameObject);
            assetService.ReleaseContainer(container);
        }
    }
}