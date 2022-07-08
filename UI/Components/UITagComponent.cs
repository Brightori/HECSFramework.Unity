using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [RequiredAtContainer(typeof(UnityTransformComponent), typeof(UnityRectTransformComponent))]
    [Serializable, Documentation(Doc.UI, Doc.Tag, "this components marks ui and provide identifier info for this ui")]
    public class UITagComponent : BaseComponent
    {
        public UIIdentifier ViewType;
    }
}