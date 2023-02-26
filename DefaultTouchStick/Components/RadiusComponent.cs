using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    public partial class RadiusComponent : BaseComponent
    {
        [Field(0)]
        public float Radius = 3;

        public int Version { get; set; }
        public bool IsDirty { get; set; }
    }
}
