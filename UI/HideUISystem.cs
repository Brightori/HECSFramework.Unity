using System;
using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Documentation(Doc.UI, "This system destroy ui on hide command")]
    [Serializable, BluePrint]
    public sealed class HideUISystem : BaseSystem, IReactCommand<HideUICommand>
    {
        public override void InitSystem()
        { }

        public void CommandReact(HideUICommand command)
        {
            EntityManager.Command(new DestroyEntityWorldCommand { Entity = Owner });
        }
    }
}