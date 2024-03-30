using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable][Documentation(Doc.HECS, Doc.Containers, "we use this component when we need just single container provider on entity")]
    public sealed class SingleContainerHolderComponent : BaseComponent
    {
        public EntityContainer EntityContainer;
    }
}