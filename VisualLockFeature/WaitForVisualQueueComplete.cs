using HECSFramework.Core;

namespace Components
{
    public struct WaitForVisualQueueComplete : IHecsJob
    {
        public VisualQueueGlobalHolderComponent VisualQueueGlobalHolderComponent;

        public WaitForVisualQueueComplete(VisualQueueGlobalHolderComponent visualQueueGlobalHolderComponent)
        {
            VisualQueueGlobalHolderComponent = visualQueueGlobalHolderComponent;
        }

        public bool IsComplete()
        {
            return VisualQueueGlobalHolderComponent.CurrentLockQueue.Count == 0;
        }

        public void Run()
        {
        }
    }
}
