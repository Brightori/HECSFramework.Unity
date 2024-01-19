using HECSFramework.Core;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using Commands;

namespace Components
{
    [Serializable, BluePrint]
    [Documentation(Doc.Visual, "this components hold asset reference to actor prefab, and this is basis functionality for actor")]
    public partial class ViewReferenceComponent : BaseComponent
    {
        public ActorViewReference ViewReference;

        public override void BeforeDispose()
        {
            if (!EntityManager.Default.TryGetSingleComponent(out OnApplicationQuitTagComponent onApplicationQuitTagComponent))
            {
                if (Owner.TryGetComponent(out ActorProviderComponent actorProviderComponent))
                {
                    if (actorProviderComponent.Actor.IsAlive())
                    {
                        EntityManager.Default.Command(new ActorViewDisposedCommand { Actor = Owner.AsActor() });
                    }
                }
            }
        }
    }

    [Serializable]
    public class ActorViewReference : ComponentReference<Actor>
    {
        public ActorViewReference(string guid) : base(guid)
        {
        }
    }
}