using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.GameLogic, Doc.Tag, Doc.HECS, "Application", "we add this tag when application quit")]
    public sealed class OnApplicationQuitTagComponent : BaseComponent, IWorldSingleComponent
    {
    }
}