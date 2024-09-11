using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Feature(Doc.VisualQueue)]
    [Serializable][Documentation(Doc.VisualQueue, "this system operates visual queue, we need this queue for waiting some visual logic and start ur own ")]
    public sealed class VisualQueueGlobalSystem : BaseSystem, IReactGlobalCommand<AddComponentReactGlobalCommand<VisualQueueLockComponent>>, IReactGlobalCommand<RemoveComponentReactGlobalCommand<VisualQueueLockComponent>> 
    {
        [Required]
        public VisualQueueGlobalHolderComponent VisualQueueGlobalHolderComponent;

        public override void InitSystem()
        {
        }

        public void CommandGlobalReact(AddComponentReactGlobalCommand<VisualQueueLockComponent> command)
        {
            command.Value.QueueIndex = VisualQueueGlobalHolderComponent.CurrentLockQueue.Count;
            VisualQueueGlobalHolderComponent.CurrentLockQueue.Add(command.Value);
        }

        public void CommandGlobalReact(RemoveComponentReactGlobalCommand<VisualQueueLockComponent> command)
        {
            VisualQueueGlobalHolderComponent.CurrentLockQueue.RemoveAt(command.Value.QueueIndex);

            for (int i = 0; i < VisualQueueGlobalHolderComponent.CurrentLockQueue.Count; i++)
            {
                VisualQueueGlobalHolderComponent.CurrentLockQueue[i].QueueIndex = i;
            }
        }
    }
}