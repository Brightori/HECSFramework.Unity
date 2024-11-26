using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.Layer, Doc.HECS, Doc.Visual, "GameObjectLayersSystem set layer to all childs of current actor")]
    public sealed class GameObjectLayersSystem : BaseSystem, IReactCommand<SetLayerCommand> 
    {
        [Required]
        public ActorChildsProviderComponent ActorChildsProviderComponent;

        public void CommandReact(SetLayerCommand command)
        {
            for (int i = 0; i < ActorChildsProviderComponent.GameObjects.Length; i++)
            {
                ActorChildsProviderComponent.GameObjects[i].gameObject.layer = command.LayerIndex; 
            }
        }

        public override void InitSystem()
        {
        }
    }
}

namespace Commands
{
    public struct SetLayerCommand : ICommand
    {
        public int LayerIndex;
    }
}