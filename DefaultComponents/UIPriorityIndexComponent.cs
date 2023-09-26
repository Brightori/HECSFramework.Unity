using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.UI, Doc.HECS, "in this component we hold priority index for sorting ui order")]
    public sealed class UIPriorityIndexComponent : BaseComponent
    {
        public int Priority;
    }
}