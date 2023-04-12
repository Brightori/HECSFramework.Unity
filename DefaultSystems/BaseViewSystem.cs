using System;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Visual, Doc.HECS, "this system its base for all systems what depends from view (when we spawn view separatly from actor), this system check when view ready")]
    public abstract class BaseViewSystem : BaseSystem, IInitAferView, IAfterEntityInit
    {
        protected bool isReady;

        public virtual void AfterEntityInit()
        {
            if (Owner.ContainsMask<ViewReadyTagComponent>() || !Owner.ContainsMask<ViewReferenceGameObjectComponent>())
                InitAferView();
        }

        public void InitAferView()
        {
            isReady = true;
            InitAfterViewLocal();
        }

        protected abstract void InitAfterViewLocal();
    }
}