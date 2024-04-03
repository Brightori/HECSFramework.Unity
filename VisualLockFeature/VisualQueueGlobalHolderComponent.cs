using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.VisualQueue, "here we hold visual queue")]
    public sealed class VisualQueueGlobalHolderComponent : BaseComponent, IWorldSingleComponent
    {
        public HECSList<VisualQueueLockComponent> CurrentLockQueue = new HECSList<VisualQueueLockComponent>();

        public override void Init()
        {
            CurrentLockQueue.Clear();
        }
    }
}