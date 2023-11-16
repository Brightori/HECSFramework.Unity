using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Provider, Doc.GameLogic, "this system gather components providers from actor and inject them to current entity")]
    public sealed class InjectComponentsFromActorSystem : BaseSystem, IHaveActor, IAfterEntityInit
    {
        public Actor Actor { get; set; }

        public void AfterEntityInit()
        {
            if (Actor.TryGetComponents(out IComponentsProvider[] provider))
            {
                foreach (var p in provider)
                {
                    p.Inject(Owner);
                }
            }
        }

        public override void InitSystem()
        {
        }
    }

    public interface IComponentsProvider
    {
        void Inject(Entity entity);
    }
}