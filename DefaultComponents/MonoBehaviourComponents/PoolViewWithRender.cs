using System;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Components
{
    [Documentation(Doc.Visual, Doc.Poolable, "this component we use for pool gameobject with disable renderer feature")]
    public class PoolViewWithRender : MonoBehaviour
    {
        [SerializeField] private AssetReference assetReference;
        
        public AssetReference AssetReference 
        { 
            get
            {
                if (assetReference == null || string.IsNullOrEmpty(assetReference.AssetGUID))
                    throw new Exception("this go doesnt have asset reference " + gameObject.name);

                return assetReference;
            }
        }

        private Renderer[] renderers;

        private void Awake()
        {
            renderers = GetComponentsInChildren<Renderer>();
        }

        public void EnableRenderers()
        {
            foreach (var r in renderers)
            {
                r.enabled = true;
            }
        }

        public void DisableRenderers()
        {
            foreach (var r in renderers)
            {
                r.enabled = false;
            }
        }
    }
}