using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Tag, Doc.UI, "when we need understand - ui showed or not, we should add this component to ui and check this component in logic")]
    public sealed class UIShowedTagComponent : BaseComponent
    {
    }
}