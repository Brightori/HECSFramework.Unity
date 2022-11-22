using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Visual, Doc.Animation, Doc.Tag, "we mark entity with this component when we should setup view logic after view complete")]
    public sealed class SetupAfterViewTagComponent : BaseComponent
    {
    }
}