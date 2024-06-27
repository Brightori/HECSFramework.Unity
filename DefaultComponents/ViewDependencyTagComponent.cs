using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Visual, Doc.HECS, Doc.Tag, "we mark entity by this tag, when we attach view and set viewready tag component externaly")]
    public sealed class ViewDependencyTagComponent : BaseComponent
    {
    }
}