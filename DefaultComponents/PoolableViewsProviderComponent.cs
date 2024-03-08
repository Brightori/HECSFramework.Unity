using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using Systems;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Poolable, "this components provides information of poolableviews, this used by PoolingSystem")]
    public sealed class PoolableViewsProviderComponent : BaseComponent, IHaveActor, IInitAfterView, IDisposable
    {
        public Actor Actor { get; set; }
        public IPoolableView[] Views =  Array.Empty<IPoolableView>();
        public HECSList<IPoolableView> RunTimeViews = new HECSList<IPoolableView>();

        public void InitAfterView()
        {
            Actor.TryGetComponents(out Views);
        }

        public override void AfterInit()
        {
            if (Owner.ContainsMask<ViewReadyTagComponent>())
                Actor.TryGetComponents(out Views);
        }

        public void Reset()
        {
            Views = Array.Empty<IPoolableView>();
        }

        public void AfterEntityInit()
        {
            
        }

        public void Dispose()
        {
            Views = null;

            foreach (var view in RunTimeViews) 
                Owner.World.GetSingleSystem<PoolingSystem>().ReleaseView(view);

            RunTimeViews.Clear();
        }
    }
}