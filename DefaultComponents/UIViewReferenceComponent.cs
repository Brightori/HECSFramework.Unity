using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable, BluePrint]
    public partial class UIViewReferenceComponent : BaseComponent
    {
        [SerializeField] private AssetReferenceT<UIActor> uiReference;

        public AssetReferenceT<UIActor> ViewReference => uiReference;
    }
}