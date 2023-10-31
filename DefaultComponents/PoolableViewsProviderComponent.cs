using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Poolable, "this components provides information of poolableviews, this used by PoolingSystem")]
    public sealed class PoolableViewsProviderComponent : BaseComponent, IHaveActor, IAfterEntityInit, IInitAfterView
    {
        public Actor Actor { get; set; }
        public IPoolableView[] Views =  Array.Empty<IPoolableView>();

        public void InitAfterView()
        {
            Actor.TryGetComponents(out Views);
        }

        public void Reset()
        {
            Views = Array.Empty<IPoolableView>();
        }

        public void AfterEntityInit()
        {
            if (Owner.ContainsMask<ViewReadyTagComponent>())
                Actor.TryGetComponents(out Views);
        }
    }
}