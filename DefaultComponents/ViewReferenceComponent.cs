using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable, BluePrint]
    public partial class ViewReferenceComponent : BaseComponent
    {
        [SerializeField] private AssetReferenceT<Actor> uiReference;
    }
}