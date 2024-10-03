using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Tag, Doc.Camera, "camera follow entity with this tag")]
    public sealed class FollowCameraTagComponent : BaseComponent
    {
    }
}