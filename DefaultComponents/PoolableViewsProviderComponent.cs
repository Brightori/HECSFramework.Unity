using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Documentation(Doc.HECS, Doc.Poolable, "this components provides information of poolableviews, this used by PoolingSystem")]
    public sealed class PoolableViewsProviderComponent : BaseComponent, IHaveActor, IInitable, IInitAfterView
    {
        public Actor Actor { get; set; }
        public IPoolableView[] Views;

        public void Init()
        {
            if (Owner.ContainsMask<ViewReadyTagComponent>())
                Actor.TryGetComponents(out Views);
        }

        public void InitAferView()
        {
            Actor.TryGetComponents(out Views);
        }
    }
}