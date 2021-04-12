using HECSFramework.Unity;

namespace HECSFramework.Core
{
    public partial class Entity
    {
        partial void ComponentAdditionalProcessing(IComponent component, IEntity owner)
        {
            if (component is IHaveActor haveActor)
                haveActor.Actor = owner as IActor;
        }

        partial void SystemAdditionalProcessing(ISystem system, IEntity owner)
        {
            if (system is IHaveActor haveActor)
                haveActor.Actor = owner as IActor;
        }
    }
}