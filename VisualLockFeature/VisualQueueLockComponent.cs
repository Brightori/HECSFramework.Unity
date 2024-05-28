using System;
using HECSFramework.Core;

namespace Components
{
    /// <summary>
    /// scenario of using this component - 
    /// GetOrAddComponent
    /// await GetWaitForQueue()
    /// after ur logic u should use Remove method on this component
    /// </summary>
    [Serializable][Documentation(Doc.VisualQueue, "this component holds global queue index for visual part of this entity, and lock count for all visual process by this entity, for proper use read summary")]
    public sealed class VisualQueueLockComponent : BaseComponent, IDisposable
    {
        public int QueueIndex;
        public int LockCount;

        public override void AfterInit()
        {
            this.AddComponentReactGlobal();
        }

        public void Dispose()
        {
            this.RemoveComponentReactGlobal();
            LockCount = 0;
        }

        /// <summary>
        /// we should wait by this to our turn
        /// and we complete visual logic, we should call Remove method
        /// </summary>  
        /// <returns></returns>
        public HECSJobRun<WaitForVisualQueue> GetWaitForQueue()
        {
            ++LockCount;
            return new WaitForVisualQueue(this).RunJob();
        }

        public void Remove()
        {
            --LockCount;

            if (LockCount <= 0)
                Owner.RemoveComponent(this);
        }
    }


    public struct WaitForVisualQueue : IHecsJob
    {
        private VisualQueueLockComponent visualQueueLockComponent;

        public WaitForVisualQueue(VisualQueueLockComponent visualQueueLockComponent)
        {
            this.visualQueueLockComponent = visualQueueLockComponent;
        }

        public bool IsComplete()
        {
            return visualQueueLockComponent.QueueIndex == 0;
        }

        public void Run()
        {
        }
    }
}