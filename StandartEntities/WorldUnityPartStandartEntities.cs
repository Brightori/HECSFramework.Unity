using Components;
using HECSFramework.Unity;
using Systems;
using Unity.IL2CPP.CompilerServices;

namespace HECSFramework.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public partial class World
    {
        partial void AddUnityWorldPart(Entity worldService)
        {
            worldService.AddHecsSystem(new AssetsServiceSystem());
        }

        partial void ComponentAdditionalProcessing(IComponent component, Entity owner, bool add)
        {
            if (component is IHaveActor haveActor)
            {
                if (add)
                    haveActor.Actor = owner.GetComponent<ActorProviderComponent>().Actor;
                else
                    haveActor.Actor = null;
            }
        }

        partial void SystemAdditionalProcessing(ISystem system, Entity owner, bool add)
        {
            if (system is IHaveActor haveActor)
            {
                if (add)
                    haveActor.Actor = owner.GetComponent<ActorProviderComponent>().Actor;
                else
                    haveActor.Actor = null;
            }
        }
    }
}