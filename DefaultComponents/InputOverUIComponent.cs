using System;
using HECSFramework.Core;
using Systems;

namespace Components
{
    [RequiredAtContainer(typeof(InputOverUISystem))]
    [Serializable][Documentation(Doc.HECS, Doc.Input, "this component holds bool  - on this moment we over ui or not")]
    public sealed partial class InputOverUIComponent : BaseComponent, IWorldSingleComponent
    {
        public bool InputOverUI;
    }
}