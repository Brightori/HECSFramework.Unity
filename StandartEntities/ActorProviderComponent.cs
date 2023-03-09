using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Documentation(Doc.HECS, "this component provide Actor from actor monobehaviour to systems and components")]
    public sealed partial class ActorProviderComponent : BaseComponent
    {
        public Actor Actor;
    }
}
