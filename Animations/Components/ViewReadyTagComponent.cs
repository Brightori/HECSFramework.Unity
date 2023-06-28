using System;
using HECSFramework.Core;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Visual, Doc.Animation, Doc.Tag, "we mark entity when view ready for interaction")]
    public sealed partial class ViewReadyTagComponent : BaseComponent
    {
        public GameObject View;
    }
}