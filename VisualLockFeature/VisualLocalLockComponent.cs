using System;
using HECSFramework.Core;

namespace Components
{
    /// <summary>
    /// scenario of using this component - 
    /// GetOrAddComponent on the feature in Action, 
    /// AddLock()
    /// after completing action use Remove()
    /// On system after Action await WaitRemove<VisualLocalLockComponent>
    /// </summary>
    [Serializable]
    [Documentation(Doc.VisualQueue, "we use this component when we need process some scenarions and wait for their completion")]
    public sealed class VisualLocalLockComponent : BaseComponent, IDisposable
    {
        public int LockCount;

        public void Dispose()
        {
            LockCount = 0;
        }

        public void AddLock()
        {
            ++LockCount;
        }

        public void Remove()
        {
            --LockCount;

            if (LockCount <= 0)
                Owner.RemoveComponent<VisualLocalLockComponent>();
        }
    }
}