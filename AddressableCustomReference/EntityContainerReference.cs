using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using System;
using UnityEngine.AddressableAssets;

namespace Helpers
{
    [Serializable]
    public class EntityContainerReference : AssetReferenceT<EntityContainer>
    {
        public EntityContainerReference(string guid) : base(guid)
        {
        }
    }
}