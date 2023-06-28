using System;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Visual, Doc.HECS, "this system its base for all systems what depends from view (when we spawn view separatly from actor), this system check when view ready")]
    public abstract class BaseViewSystem : BaseSystem, IInitAfterView, IAfterEntityInit
    {
        protected bool isReady;

        public virtual void AfterEntityInit()
        {
            if (Owner.ContainsMask<ViewReadyTagComponent>() || !Owner.ContainsMask<ViewReferenceGameObjectComponent>())
                InitAfterView();
        }

        public void InitAfterView()
        {
            isReady = true;
            InitAfterViewLocal();
        }

        public void Reset()
        {
            isReady = false;
            ResetLocal();
        }

        protected abstract void InitAfterViewLocal();
        protected abstract void ResetLocal();
    }
}