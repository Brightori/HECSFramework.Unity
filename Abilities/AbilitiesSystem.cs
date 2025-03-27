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

        partial void ProcessViewReady(Entity entity)
        {
            if (Owner.TryGetComponent(out ViewReadyTagComponent viewReadyTagComponent))
            {
                entity.GetOrAddComponent<ViewReadyTagComponent>().View = viewReadyTagComponent.View;
            }
        }

        partial void ClientInit()
        {
            abilitiesHolderComponent.LoadDefaultAbilities();
        }
    }
}