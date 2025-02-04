using System;
using System.Collections.Generic;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Documentation(Doc.GameLogic, "Эта система живет в самом мире, отвечает за то что после всех апдейтов вызовется эта система, и почистит ентити которые мы просим удалить")]
    public sealed partial class DestroyEntityWorldSystem : IReactGlobalCommand<DeleteActorCommand>
    {
        private Queue<Actor> actorsForDelete = new Queue<Actor>();

        private PoolingSystem poolingSystem;

        partial void UnityInit()
        {
            poolingSystem = Owner.World.GetSingleSystem<PoolingSystem>();
            Owner.World.GlobalUpdateSystem.FinishUpdate += ReactUnityPart;
        }

        private void ReactUnityPart()
        {
            while (actorsForDelete.TryDequeue(out Actor actor))
            {
                if (actor != null)
                    poolingSystem.Release(actor);
            }
        }

        partial void UnityDispose()
        {
            Owner.World.GlobalUpdateSystem.FinishUpdate -= ReactUnityPart;
        }

        public void CommandGlobalReact(DeleteActorCommand command)
        {
            actorsForDelete.Enqueue(command.Actor);
        }
    }
}

namespace Commands
{
    public struct DeleteActorCommand : IGlobalCommand
    {
        public Actor Actor;
    }
}