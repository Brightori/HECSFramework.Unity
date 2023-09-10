using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Tag, Doc.Containers, "we mark by this component containers when we need ignore reference containers or need ignore them when we collect containers of the desired type")]
    public sealed class IgnoreReferenceContainerTagComponent : BaseComponent
    {
       
    }
}