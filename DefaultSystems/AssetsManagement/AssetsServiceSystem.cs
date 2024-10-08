using System;
using System.Collections.Generic;
using AssetsManagement.Containers;
using Commands;
using HECSFramework.Core;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Helpers, "Global system to managing assets")]
    public sealed class AssetsServiceSystem : BaseSystem
    {
        private const int MAX_RETRY_DELAY = 60;

        private readonly Dictionary<AssetReference, UniTask> assetsLoadsMap = new();
        private readonly Dictionary<AssetReference, object> assetsContainersCache = new();
        private readonly Dictionary<AssetReference, int> containersRefsCount = new();

        private int exceptionsCount;

        public override void InitSystem()
        {
        }

        public async UniTask<AssetRefContainer<TRef, TObject>> GetContainer<TRef, TObject>(TRef reference)
            where TRef : AssetReference
            where TObject : Object
        {
            if (assetsLoadsMap.TryGetValue(reference, out var value))
            {
                await value;
            }
            else if (!assetsContainersCache.ContainsKey(reference))
            {
                await PreloadContainer<TRef, TObject>(reference);
            }

            containersRefsCount[reference]++;
            return assetsContainersCache[reference] as AssetRefContainer<TRef, TObject>;
        }

        public void ReleaseContainer<TRef, TObject>(AssetRefContainer<TRef, TObject> refContainer)
            where TRef : AssetReference
            where TObject : Object
        {
            TRef reference = refContainer.Reference;
            if (!assetsContainersCache.ContainsKey(reference))
            {
                Debug.LogError($"Cannot find container with provided ref {reference}");
                return;
            }

            containersRefsCount[reference]--;

            int assetsInstancesRefsCount = refContainer.RefsCount;
            int assetContainerRefsCount = containersRefsCount[reference];

            if (assetsInstancesRefsCount == 0 && assetContainerRefsCount == 0)
            {
                containersRefsCount.Remove(reference);
                assetsContainersCache.Remove(reference);
                reference.ReleaseAsset();
            }
        }

        private async UniTask PreloadContainer<TRef, TObject>(
            TRef reference,
            UniTaskCompletionSource loadingTCS = null
        )
            where TRef : AssetReference
            where TObject : Object
        {
            if (reference == null)
                throw new InvalidOperationException("asset reference is null");

            if (loadingTCS == null)
            {
                loadingTCS = new UniTaskCompletionSource();
                assetsLoadsMap[reference] = loadingTCS.Task.Preserve();
            }

            try
            {
                await reference.LoadAssetAsync<TObject>().ToUniTask();

                exceptionsCount = 0;
                AssetRefContainer<TRef, TObject> refContainer = new AssetRefContainer<TRef, TObject>(reference);
                assetsContainersCache[reference] = refContainer;
                containersRefsCount[reference] = 0;

                assetsLoadsMap.Remove(reference);
                loadingTCS.TrySetResult();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                exceptionsCount++;
                int delayTime = Mathf.Clamp((int)Mathf.Pow(2, exceptionsCount), 0, MAX_RETRY_DELAY) * 1000;
                await UniTask.Delay(delayTime);
                await PreloadContainer<TRef, TObject>(reference, loadingTCS);
            }
        }
    }
}