using System;
using Commands;
using HECSFramework.Core;

namespace Components
{
    [Serializable, RequiredAtContainer(typeof(UnityTransformComponent))]
    [Documentation(Doc.UI, Doc.Tag, "This component marks main canvas for placing ui")]
    public class MainCanvasTagComponent : BaseComponent, IAfterEntityInit
    {
        public void AfterEntityInit()
        {
            Owner.World.Command(new CanvasReadyCommand());
        }
    }
}