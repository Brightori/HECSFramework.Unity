using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable, BluePrint]
    public partial class UIViewReferenceComponent : BaseComponent
    {
        [SerializeField] private UIViewReference uiReference;

        public UIViewReference ViewReference => uiReference;
    }
}