using Commands;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    public sealed partial class AbilitiesSystem : BaseSystem, IReactCommand<ViewReadyCommand>
    {
        public void CommandReact(ViewReadyCommand command)
        {
            foreach (var a in abilitiesHolderComponent.Abilities)
            {
                a.Command(command);
            }
        }

        partial void ClientInit()
        {
            foreach (var abilityContainer in abilitiesHolderComponent.AbilitiesContainers)
            {
                var newAbil = abilityContainer.GetEntity();
                newAbil.GetOrAddComponent<AbilityOwnerComponent>().AbilityOwner = Owner;
                abilitiesHolderComponent.AddAbility(newAbil);
            }
        }
    }
}