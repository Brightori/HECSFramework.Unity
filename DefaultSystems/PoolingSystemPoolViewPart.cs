using System.Collections.Generic;
using Components;
using HECSFramework.Core;
using UnityEngine.AddressableAssets;

namespace Systems
{
    public delegate void ViewWithRendererCallBack(PoolViewWithRender poolViewWithRender);

    public partial class PoolingSystem : BaseSystem
    {
        private Dictionary<AssetReference, Stack<PoolViewWithRender>> poolViews = new Dictionary<AssetReference, Stack<PoolViewWithRender>>(8);

        public void GetViewWithRenderer(AssetReference assetReference, ViewWithRendererCallBack viewCallBack)
        {
            if (!poolViews.ContainsKey(assetReference))
            {
                poolViews.Add(assetReference, new Stack<PoolViewWithRender>(16));
            }

            if (poolViews.TryGetValue(assetReference, out var pool))
            {
                if (pool.TryPop(out var result))
                {
                    viewCallBack?.Invoke(result);
                    return;
                }
            }
            else
                poolViews.Add(assetReference, new Stack<PoolViewWithRender>(16));

            GetView(assetReference, viewCallBack);
        }

        public void Release(PoolViewWithRender poolViewWithRender)
        {
            if (poolViews.TryGetValue(poolViewWithRender.AssetReference, out var pool))
            {
                pool.Push(poolViewWithRender);
            }
            else
                poolViews.Add(poolViewWithRender.AssetReference, new Stack<PoolViewWithRender>(16));
        }

        private async void GetView(AssetReference assetReference, ViewWithRendererCallBack viewWithRendererCallBack)
        {
            var needed = await assetReference.InstantiateAsync().Task;
            var pooledView = needed.GetComponent<PoolViewWithRender>();
            viewWithRendererCallBack?.Invoke(pooledView);
        }
    }
}
