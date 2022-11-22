using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    public sealed partial class AbilitiesSystem : BaseSystem
    {
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