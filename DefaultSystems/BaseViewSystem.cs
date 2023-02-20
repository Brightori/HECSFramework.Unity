using System;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Visual, Doc.HECS, "this system its base for all systems what depends from view (when we spawn view separatly from actor), this system check when view ready")]
    public abstract class BaseViewSystem : BaseSystem, IInitAferView
    {
        protected bool isReady;

        public void InitAferView()
        {
            isReady = true;
        }

        protected abstract void InitAfterViewLocal();
    }
}