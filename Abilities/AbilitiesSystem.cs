using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    public sealed partial class AbilitiesSystem : BaseSystem, IReactCommand<AddComponentReactLocalCommand<ViewReadyTagComponent>>
    {
        public void CommandReact(AddComponentReactLocalCommand<ViewReadyTagComponent> command)
        {
            foreach (var a in abilitiesHolderComponent.Abilities)
            {
                a.GetOrAddComponent<ViewReadyTagComponent>();
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