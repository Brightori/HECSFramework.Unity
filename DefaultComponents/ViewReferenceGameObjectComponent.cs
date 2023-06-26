using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine.AddressableAssets;

#pragma warning disable

namespace Components
{
    [Serializable, BluePrint]
    public partial class ViewReferenceGameObjectComponent : BaseComponent, IValidate
    {
        public AssetReferenceGameObject ViewReference;

        public bool IsValid()
        {
#if UNITY_EDITOR
            return ViewReference != null && ViewReference.editorAsset != null && AddressablesHelpers.IsAssetAddressable(ViewReference.AssetGUID);
#endif
            return true;
        }
    }
}