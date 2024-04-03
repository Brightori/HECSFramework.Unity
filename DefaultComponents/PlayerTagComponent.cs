using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Tag, Doc.Player, Doc.HECS, "PlayerTagComponent")]
    public sealed class PlayerTagComponent : BaseComponent, IWorldSingleComponent
    {
    }
}