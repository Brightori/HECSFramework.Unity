using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Serializable, Documentation(Doc.UI, Doc.Tag, "This component holds identifier for additional canvases, we use this canvases for optimisation or drawing purpose")]
    public sealed class AdditionalCanvasTagComponent : BaseComponent
    {
        public AdditionalCanvasIdentifier AdditionalCanvasIdentifier;
    }
}