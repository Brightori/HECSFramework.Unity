using Commands;
using Components;
using HECSFramework.Core;

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
            abilitiesHolderComponent.LoadDefaultAbilities();
        }
    }
}