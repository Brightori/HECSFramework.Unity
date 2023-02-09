using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.NONE, "")]
    public sealed class CheckTwoComponent : BaseComponent
    {
        public float CheckValue = 2;
    }
}