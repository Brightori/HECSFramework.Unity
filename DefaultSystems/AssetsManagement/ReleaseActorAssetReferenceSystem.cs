using System;
using Commands;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.GameLogic, "listen to disposing actor and release by assetServiceSystem and pooling system")]
    public sealed class ReleaseActorAssetReferenceSystem : BaseSystem, IReactGlobalCommand<ActorViewDisposedCommand>
    {
        public override void InitSystem()
        {
        }

        public void CommandGlobalReact(ActorViewDisposedCommand command)
        {
            
        }
    }
}