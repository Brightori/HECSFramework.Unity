using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Serializable]
    [Feature("InjectComponentsFromActor")]
    [Documentation(Doc.HECS, Doc.Provider, Doc.GameLogic, "this system gather components providers from actor and inject them to current entity, actor shoud have child or MonoBehs with implementation of interface IComponentsProvider")]
    public sealed class InjectComponentsFromActorSystem : BaseSystem, IHaveActor 
    {
        public Actor Actor { get; set; }

        public override void InitSystem()
        {
            if (Actor.TryGetComponents(out IComponentsProvider[] provider))
            {
                foreach (var p in provider)
                {
                    p.Inject(Owner);
                }
            }
        }
    }

    public interface IComponentsProvider
    {
        void Inject(Entity entity);
    }
}