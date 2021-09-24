using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Serializable, BluePrint]
    public partial class ViewReferenceGameObjectComponent : BaseComponent
    {
        public AssetReferenceGameObject ViewReference;
    }
}