using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    public sealed partial class AbilitiesSystem : BaseSystem, IReactComponentLocal<ViewReadyTagComponent>
    {
        public void ComponentReact(ViewReadyTagComponent component, bool isAdded)
        {
            if (isAdded)
            {
                foreach (var a in abilitiesHolderComponent.Abilities)
                {
                    a.GetOrAddComponent<ViewReadyTagComponent>();
                }
            }
        }

        partial void ClientInit()
        {
            foreach (var abilityContainer in abilitiesHolderComponent.AbilitiesContainers)
            {
                var newAbil = abilityContainer.GetEntity(world: Owner.World);
                newAbil.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;
                abilitiesHolderComponent.AddAbility(newAbil);
            }
        }
    }
}