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

        public void ProcessSystemsAndComponentsWithActor(IActor actor)
        {
            foreach (var c in GetAllComponents)
            {
                if (c != null)
                    c.Owner = actor;

                if (c is IHaveActor haveActor)
                    haveActor.Actor = actor;
            }

            foreach (var s in systems)
            {
                s.Owner = actor;

                if (s is IHaveActor haveActor)
                    haveActor.Actor = actor;
            }
        }
    }
}