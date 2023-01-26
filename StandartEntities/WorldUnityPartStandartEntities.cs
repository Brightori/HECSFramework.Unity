using Components;
using HECSFramework.Unity;
using Unity.IL2CPP.CompilerServices;

namespace HECSFramework.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public partial class World
    {
        partial void ComponentAdditionalProcessing(IComponent component, Entity owner)
        {
            if (component is IHaveActor haveActor)
                haveActor.Actor = owner.GetOrAddComponent<ActorProviderComponent>().Actor;
        }

        partial void SystemAdditionalProcessing(ISystem system, Entity owner)
        {
            if (system is IHaveActor haveActor)
                haveActor.Actor = owner.GetOrAddComponent<ActorProviderComponent>().Actor;
        }
    }
}