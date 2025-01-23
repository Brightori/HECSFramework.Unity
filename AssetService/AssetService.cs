using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using Debug = UnityEngine.Debug;

namespace Systems
{
    public delegate void ProgressUpdate(float progress);
    public delegate void DownloadSizeUpdate(long downLoadedSize, long overallSize);

    public class AssetService : BaseSystem
    {
        protected HashSet<AssetContainer> assetsContainers = new HashSet<AssetContainer>(64);
        private readonly Dictionary<int, AssetContainer> objectToContainer = new Dictionary<int, AssetContainer>(32);

        private Queue<AssetContainer> releasedContainers = new Queue<AssetContainer>(32);

        public override void InitSystem()
        {
        }

        #region GetAsset

        public async UniTask<T> GetAsset<T>(string resourceName, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new Exception("[AssetService] GetAsset invalid name provided ");

#if UNITY_EDITOR
            Debug.Log($"[AssetService] GetAsset {resourceName}");
#endif

            if (AssetContainerHolder<T>.KeyToAssetContainer.TryGetValue(resourceName, out var container) && !container.IsReleased)
            {
                return await container.GetAsset(resourceName, objectToContainer, assetsContainers, progress, isForceRelease, cancellationToken);
            }

            var newcontainer = AssetContainerHolder<T>.KeyToAssetContainer[resourceName] = new AssetContainer<T>(resourceName, objectToContainer, isForceRelease);
            return await newcontainer.LoadAsset(resourceName, objectToContainer, assetsContainers, progress, isForceRelease, cancellationToken);
        }

        public async UniTask<(bool, T)> TryGetAsset<T>(string resourceName, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new Exception("[AssetService] GetAsset invalid name provided ");

#if UNITY_EDITOR
            Debug.Log($"[AssetService] GetAsset {resourceName}");
#endif

            var resourceLocation = await Addressables.LoadResourceLocationsAsync(resourceName, typeof(T)).Task;

            if (resourceLocation.Count == 0)
                return (false, null);

            if (AssetContainerHolder<T>.KeyToAssetContainer.TryGetValue(resourceName, out var container) && !container.IsReleased)
            {
                var result = await container.GetAsset(resourceName, objectToContainer, assetsContainers, progress, isForceRelease, cancellationToken);
                return (true, result);
            }
            else
            {
                var newcontainer = AssetContainerHolder<T>.KeyToAssetContainer[resourceName] = new AssetContainer<T>(resourceName, objectToContainer, isForceRelease);
                var result = await newcontainer.LoadAsset(resourceName, objectToContainer, assetsContainers, progress, isForceRelease, cancellationToken);
                return (true, result);
            }
        }

        public async UniTask<T> GetAsset<T>(AssetReference assetReference, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (assetReference == null)
                throw new Exception("[AssetService] GetAsset invalid name provided ");


            if (AssetContainerHolder<T>.AssetReferenceToAssetContainer.TryGetValue(assetReference, out var container) && !container.IsReleased)
            {
                return await container.GetAsset(assetReference, objectToContainer, assetsContainers, progress, isForceRelease, cancellationToken);
            }

            var newcontainer = AssetContainerHolder<T>.AssetReferenceToAssetContainer[assetReference] = new AssetContainer<T>(assetReference.AssetGUID, objectToContainer, isForceRelease);
            return await newcontainer.LoadAsset(assetReference, objectToContainer, assetsContainers, progress, isForceRelease, cancellationToken);
        }


        public async UniTask<IList<T>> GetAssetsByTag<T>(string label) where T : UnityEngine.Object
        {
            var t = await Addressables.LoadResourceLocationsAsync(label, typeof(T)).Task;
            var list = new List<UniTask<T>>(t.Count);

            foreach (var rl in t)
            {
                list.Add(GetAsset<T>(rl.PrimaryKey));
            }

            var result = await UniTask.WhenAll(list);
            return result;
        }

        public T GetAssetSync<T>(string resourceName, bool isForceRelease = false) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            Debug.Log($"[AssetService] GetAsset {resourceName}");
#endif

            if (AssetContainerHolder<T>.KeyToAssetContainer.TryGetValue(resourceName, out var container) && !container.IsReleased && container.IsReady)
            {
                return container.CurrentObject;
            }

            try
            {
                var task = Addressables.LoadAssetAsync<T>(resourceName);
                var result = task.WaitForCompletion();

                if (AssetContainerHolder<T>.KeyToAssetContainer.TryGetValue(resourceName, out var assetContainer))
                {
                    if (!assetContainer.IsReleased)
                        return result;
                }

                var newContainer = new AssetContainer<T>(result, resourceName, task, objectToContainer, isForceRelease);

                AssetContainerHolder<T>.KeyToAssetContainer[resourceName] = newContainer;
                objectToContainer.Add(result.GetInstanceID(), newContainer);
                assetsContainers.Add(newContainer);
                return result;
            }
            catch (Exception ex)
            {
                Debug.LogError("[AssetService] addressable load failed " + ex.ToString());
            }

            throw new Exception("[AssetService] GetAssetSync > we dont have addressable for " + resourceName);
        }
        #endregion

        #region GetComponentFromAsset
        public T GetComponentFromAssetSync<T>(string resourceName, bool isForceRelease = false) where T : Component
        {
            var prefab = GetAssetSync<GameObject>(resourceName, isForceRelease);
            return prefab.GetComponent<T>();
        }
        public async UniTask<T> GetComponentFromAsset<T>(string resourceName, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : Component
        {
            var prefab = await GetAsset<GameObject>(resourceName, progress, isForceRelease, cancellationToken);
            return prefab.GetComponent<T>();
        }
        #endregion

        #region GetComponentFromInstance
        public async UniTask<T> GetComponentFromGameObjectInstance<T>(string resourceName, Transform parent = null, Vector3 pos = default, Quaternion rotation = default, ProgressUpdate progress = null, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default) where T : Component
        {
            var task = await GetAssetInstance(resourceName, pos, rotation, parent, progress, newAsyncInstatiate, cancellationToken);
            return task.GetComponent<T>();
        }

        public T GetComponentFromGameObjectInstanceSync<T>(string resourceName, Transform parent = null, Vector3 pos = default, Quaternion rotation = default, ProgressUpdate progress = null, CancellationToken cancellationToken = default) where T : Component
        {
            var task = GetAssetInstanceSync<GameObject>(resourceName, parent, pos, rotation, progress, cancellationToken);
            return task.GetComponent<T>();
        }
        #endregion

        #region GetAssetInstance

        public async UniTask<GameObject> GetAssetInstance(string resourceName)
        {
            var needed = await GetAsset<GameObject>(resourceName, null);
            GameObject result = null;

            result = UnityEngine.Object.Instantiate(needed);

            if (AssetContainerHolder<GameObject>.KeyToAssetContainer[resourceName].RegisterObject(result))
                objectToContainer.Add(result.GetInstanceID(), AssetContainerHolder<GameObject>.KeyToAssetContainer[resourceName]);

            return result;
        }

        public async UniTask<GameObject> GetAssetInstance(string resourceName, Transform parent, CancellationToken cancellationToken = default)
        {
            var needed = await GetAsset<GameObject>(resourceName, cancellationToken: cancellationToken);
            GameObject result = null;

            result = UnityEngine.Object.Instantiate(needed, parent);

            if (AssetContainerHolder<GameObject>.KeyToAssetContainer[resourceName].RegisterObject(result))
                objectToContainer.Add(result.GetInstanceID(), AssetContainerHolder<GameObject>.KeyToAssetContainer[resourceName]);

            return result;
        }

        public async UniTask<GameObject> GetAssetInstance(string resourceName, Vector3 pos, Quaternion rotation, Transform parent = null, ProgressUpdate progress = null, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default)
        {
            var needed = await GetAsset<GameObject>(resourceName, progress);
            GameObject result = null;

            if (newAsyncInstatiate)
            {
                var instance = UnityEngine.Object.InstantiateAsync(needed, parent, pos, rotation);

                while (!instance.isDone)
                {
                    progress?.Invoke(instance.progress);
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    UnityEngine.Object.Destroy(instance.Result[0]);
                    throw new Exception("[AssetService] GetAssetInstance canceled: " + resourceName);
                }

                result = instance.Result[0];
            }
            else
                result = UnityEngine.Object.Instantiate(needed, pos, rotation, parent);

            if (AssetContainerHolder<GameObject>.KeyToAssetContainer[resourceName].RegisterObject(result))
                objectToContainer.Add(result.GetInstanceID(), AssetContainerHolder<GameObject>.KeyToAssetContainer[resourceName]);

            return result;
        }

        public async UniTask<GameObject> GetAssetInstance(AssetReference assetReference, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default)
        {
            return await GetAssetInstance(assetReference, Vector3.zero, Quaternion.identity, null, null, newAsyncInstatiate, cancellationToken);
        }

        public async UniTask<GameObject> GetAssetInstance(AssetReference assetReference, Vector3 pos, Quaternion rotation, Transform parent = null, ProgressUpdate progress = null, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default)
        {
            var needed = await GetAsset<GameObject>(assetReference, progress);
            GameObject result = null;

            if (newAsyncInstatiate)
            {
                var instance = UnityEngine.Object.InstantiateAsync(needed, parent, pos, rotation);

                while (!instance.isDone)
                {
                    progress?.Invoke(instance.progress);
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    UnityEngine.Object.Destroy(instance.Result[0]);
                    throw new Exception("[AssetService] GetAssetInstance canceled: " + assetReference.RuntimeKey);
                }

                result = instance.Result[0];
            }
            else
                result = UnityEngine.Object.Instantiate(needed, pos, rotation, parent);

            if (AssetContainerHolder<GameObject>.AssetReferenceToAssetContainer[assetReference].RegisterObject(result))
                objectToContainer.Add(result.GetInstanceID(), AssetContainerHolder<GameObject>.AssetReferenceToAssetContainer[assetReference]);

            return result;
        }

        public async UniTask<GameObject[]> GetAssetsInstance(AssetReference assetReference, Vector3 pos, Quaternion rotation, Transform parent = null, ProgressUpdate progress = null, CancellationToken cancellationToken = default, int neededCount = 1)
        {
            if (neededCount == 0)
                return Array.Empty<GameObject>();

            var needed = await GetAsset<GameObject>(assetReference, progress);

                var instance = UnityEngine.Object.InstantiateAsync(needed, neededCount, parent, pos, rotation);

                while (!instance.isDone)
                {
                    progress?.Invoke(instance.progress);
                    await UniTask.Yield();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    UnityEngine.Object.Destroy(instance.Result[0]);
                    throw new Exception("[AssetService] GetAssetInstance canceled: " + assetReference.RuntimeKey);
                }


            for (int i = 0; i < instance.Result.Length; i++)
            {
                if (AssetContainerHolder<GameObject>.AssetReferenceToAssetContainer[assetReference].RegisterObject(instance.Result[i]))
                    objectToContainer.Add(instance.Result[i].GetInstanceID(), AssetContainerHolder<GameObject>.AssetReferenceToAssetContainer[assetReference]);
            }

            return instance.Result;
        }

        public GameObject GetAssetInstanceSync(string resourceName)
        {
            return GetAssetInstanceSync<GameObject>(resourceName, null);
        }

        public T GetAssetInstanceSync<T>(string resourceName, Transform parent) where T : UnityEngine.Object
        {
            var needed = GetAssetSync<T>(resourceName);
            var instance = UnityEngine.Object.Instantiate(needed, parent);
            var result = instance;

            if (AssetContainerHolder<T>.KeyToAssetContainer[resourceName].RegisterObject(result))
                objectToContainer.Add(result.GetInstanceID(), AssetContainerHolder<T>.KeyToAssetContainer[resourceName]);

            return result;
        }

        public T GetAssetInstanceSync<T>(string resourceName, Transform parent = null, Vector3 pos = default, Quaternion rotation = default, ProgressUpdate progress = null, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var needed = GetAssetSync<T>(resourceName);
            var instance = UnityEngine.Object.Instantiate(needed, pos, rotation, parent);
            var result = instance;

            if (AssetContainerHolder<T>.KeyToAssetContainer[resourceName].RegisterObject(result))
                objectToContainer.Add(result.GetInstanceID(), AssetContainerHolder<T>.KeyToAssetContainer[resourceName]);

            return result;
        }
        #endregion

        #region WarmUp
        public async UniTask WarmUpAssetsProgress<T>(string bundleNameOrTag, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var list = await Addressables.LoadResourceLocationsAsync(bundleNameOrTag, Addressables.MergeMode.Union, typeof(T)).Task;
            await ProcessWarmUpAssets<T>(list, progress);
        }

        public async UniTask WarmUpAssetsProgress<T>(IEnumerable<string> resources, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var list = await Addressables.LoadResourceLocationsAsync(resources, Addressables.MergeMode.Union, typeof(T)).Task;
            await ProcessWarmUpAssets<T>(list, progress);
        }

        public async UniTask WarmUpAssets<T>(string bundleNameOrTag, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var list = await Addressables.LoadResourceLocationsAsync(bundleNameOrTag, Addressables.MergeMode.Union, typeof(T)).Task;

            await ProcessWarmUpAssets<T>(list, progress);
        }

        public async UniTask WarmUpAssets<T>(IEnumerable<string> resources, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var list = await Addressables.LoadResourceLocationsAsync(resources, Addressables.MergeMode.Union, typeof(T)).Task;
            await ProcessWarmUpAssets<T>(list, progress);
        }

        private async UniTask ProcessWarmUpAssets<T>(IList<IResourceLocation> list, ProgressUpdate progress, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var resourceCount = list.Count;

            var tasks = new List<UniTask>(resourceCount);

            if (progress != null)
            {
                List<float> progressList = new List<float>(resourceCount);

                void ProgressUpdatedHandler(float updatedProgress, int currentIndex)
                {
                    progressList[currentIndex] = updatedProgress;
                    progress?.Invoke(progressList.Sum() / resourceCount);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    IResourceLocation r = list[i];
                    var index = i;
                    progressList.Add(0);
                    tasks.Add(GetAsset<T>(r.PrimaryKey, (x) => ProgressUpdatedHandler(x, index)));
                }

                await UniTask.WhenAll(tasks);
            }
            else
            {
                for (int i = 0; i < list.Count; i++)
                {
                    IResourceLocation r = list[i];
                    tasks.Add(GetAsset<T>(r.PrimaryKey));
                }

                await UniTask.WhenAll(tasks);
            }
        }

        #endregion

        #region LoadRemote
        public async UniTask LoadRemote(string groupNameOrTag, ProgressUpdate progress, DownloadSizeUpdate downloadSizeUpdate)
        {
            try
            {
                var handle = Addressables.DownloadDependenciesAsync(groupNameOrTag);
                long resourcesSize = handle.GetDownloadStatus().TotalBytes;

#if UNITY_EDITOR
                Debug.LogWarning($"[AssetService] DownLoadSize of remote {groupNameOrTag} bundle is {resourcesSize}");
#endif

                try
                {
                    while (!handle.IsDone)
                    {
                        await UniTask.Yield();

                        long totalDownloadedBytes = handle.GetDownloadStatus().DownloadedBytes;
                        var part = resourcesSize > 0 ? totalDownloadedBytes / resourcesSize : 1f;
                        progress?.Invoke(part);
                        downloadSizeUpdate?.Invoke(totalDownloadedBytes, resourcesSize);
#if UNITY_EDITOR
                        Debug.Log($"[BundleLoadingProcess] '{groupNameOrTag}' Loading..., Progress: {totalDownloadedBytes}/{resourcesSize} ({totalDownloadedBytes.FormatBytes()}/{resourcesSize.FormatBytes()})");
#endif
                    }

                    if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        Debug.LogError($"Downloading error: {GetDownloadError(handle)}");
                    }
                    else
                    {
                        var downloadStatus = handle.GetDownloadStatus();
                        if (downloadStatus.DownloadedBytes < downloadStatus.TotalBytes)
                        {
                            Debug.LogError("Bundle downloading successfuly finished, but downloaded less than needed.");
                        }
                    }
                }
                finally
                {
                    Addressables.Release(handle);
                }
            }
            finally
            {
                progress?.Invoke(1);
            }
        }

        public async UniTask LoadRemote(IEnumerable<string> resources, ProgressUpdate progress = null, DownloadSizeUpdate downloadSizeUpdate = null)
        {
            var resourcesCount = resources.Count();
            var list = new List<UniTask>(resourcesCount);
            List<float> progressList = new List<float>(resourcesCount);
            List<long> downLoadSize = new List<long>(resourcesCount);
            int index = 0;

            var resourcesLocations = await Addressables.LoadResourceLocationsAsync(resources, Addressables.MergeMode.Union).Task;
            var overallSize = await Addressables.GetDownloadSizeAsync(resourcesLocations).Task;

#if UNITY_EDITOR
            Debug.LogWarning($"[AssetService] DownLoadSize of all dependencies is {overallSize}");
#endif

            try
            {
                foreach (var r in resources)
                {
                    progressList.Add(0);
                    downLoadSize.Add(0);
                    var tempIndex = index;
                    list.Add(LoadRemote(r, (x) => ProgressUpdatedHandler(x, tempIndex), (z, y) => DownloadSizeUpdatedHandler(z, y, tempIndex)));
                    index++;
                }

                void ProgressUpdatedHandler(float updatedProgress, int currentIndex)
                {
                    progressList[currentIndex] = updatedProgress;
                    progress?.Invoke(progressList.Sum() / resourcesCount);
                }

                void DownloadSizeUpdatedHandler(long currentSize, long totalSize, int currentIndex)
                {
                    downLoadSize[currentIndex] = currentSize;
                    downloadSizeUpdate?.Invoke(downLoadSize.Sum(), overallSize);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("[AssetService] addressable load failed " + ex.ToString());
            }

            await UniTask.WhenAll(list);
        }

        private string GetDownloadError(AsyncOperationHandle fromHandle)
        {
            if (fromHandle.Status != AsyncOperationStatus.Failed)
                return null;

            Exception e = fromHandle.OperationException;
            while (e != null)
            {
                if (e is RemoteProviderException remoteException)
                    return remoteException.WebRequestResult.Error;
                e = e.InnerException;
            }

            return null;
        }
        #endregion

        #region ReleaseAndUnload
        public bool Release<T>(T obj, bool force = false) where T : UnityEngine.Object
        {
            if (obj == null)
                return false;

            if (objectToContainer.TryGetValue(obj.GetInstanceID(), out var container))
            {
                objectToContainer.Remove(obj.GetInstanceID());
                var release = container.ReleaseObject(obj);

                if (container.IsReleased)
                {
                    ReleaseContainer(container);
                }

                return release;
            }

            UnityEngine.Object.Destroy(obj);
            return false;
        }

        private void ReleaseContainer(AssetContainer assetContainer)
        {
            objectToContainer.Remove(assetContainer.ObjectID);
        }

        public void UnloadUnusedResources(bool forceRelease = false)
        {
            foreach (var container in assetsContainers)
            {
                container.UpdateResourceStatus(objectToContainer, forceRelease);

                if (container.IsReleased)
                    releasedContainers.Enqueue(container);
            }

            while (releasedContainers.TryDequeue(out var container))
                assetsContainers.Remove(container);
        }

        public async UniTask ReleaseByTag(string tag)
        {
            var labelAssets = await Addressables.LoadResourceLocationsAsync(tag).Task;

            foreach (var l in labelAssets)
            {
                if (AssetContainerHolder<GameObject>.KeyToAssetContainer.TryGetValue(l.PrimaryKey, out var container))
                {
                    container.UnlockForceRelease();
                    container.UpdateResourceStatus(objectToContainer);
                }
            }
        }

       

        //~AssetService() 
        //{ 
        //    foreach(var container in assetsContainers)
        //        container.Dispose();
        //}

        #endregion
    }

    #region AssetContainerAndHelpers

    public abstract class AssetContainer : IDisposable, IEquatable<AssetContainer>
    {
        public int ObjectID { get; protected set; }
        public readonly string ObjectKey;
        protected Dictionary<int, AssetContainer> objectToContainer;
        protected AssetReference assetReference;

        public abstract bool IsReady { get; }


        [ShowInInspector]
        protected int counter;

        protected AssetContainer(int objectID, string objectKey, Dictionary<int, AssetContainer> objectToContainer)
        {
            ObjectID = objectID;
            ObjectKey = objectKey;
            this.objectToContainer = objectToContainer;
        }

        protected AssetContainer(string objectKey, Dictionary<int, AssetContainer> objectToContainer)
        {
            ObjectKey = objectKey;
            this.objectToContainer = objectToContainer;
        }

        public bool IsReleased { get; protected set; }

        public abstract bool ReleaseObject(UnityEngine.Object obj, bool forceRelease = false);
        public abstract void LockContainer();
        public abstract void UnlockForceRelease();
        public abstract void UpdateResourceStatus(Dictionary<int, AssetContainer> objectToContainers, bool forceRelease = false);
        public abstract void Dispose();

        public override bool Equals(object obj)
        {
            return obj is AssetContainer container &&
                   ObjectID == container.ObjectID &&
                   ObjectKey == container.ObjectKey;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ObjectID, ObjectKey);
        }

        public bool Equals(AssetContainer container)
        {
            return ObjectID == container.ObjectID &&
                   ObjectKey == container.ObjectKey;
        }
    }

    public static class AssetContainerHolder<T> where T : UnityEngine.Object
    {
        public static readonly Dictionary<string, AssetContainer<T>> KeyToAssetContainer = new Dictionary<string, AssetContainer<T>>(32);
        public static readonly Dictionary<AssetReference, AssetContainer<T>> AssetReferenceToAssetContainer = new Dictionary<AssetReference, AssetContainer<T>>(32);
    }

    public class AssetContainer<T> : AssetContainer where T : UnityEngine.Object
    {
        private Dictionary<int, T> activeObjects;
        private AsyncOperationHandle<T> asyncOperationHandle;

        private bool needforceRelease;
        private bool isDisposed;

        [ShowInInspector]
        public T CurrentObject { get; private set; }
        public override bool IsReady => isReady;

        private bool isReady;
        private float fillProgress;


        /// <param name="asyncOperation"></param>
        /// <param name="forceRelease">if this argument true, we release this asset only when  we use force release flag on release object</param>
        /// <param name="size"></param>
        public AssetContainer(T obj, string objectKey, AsyncOperationHandle<T> asyncOperation, Dictionary<int, AssetContainer> objectToContainer, bool forceRelease, int size = 1) : base(obj.GetInstanceID(), objectKey, objectToContainer)
        {
            asyncOperationHandle = asyncOperation;
            needforceRelease = forceRelease;
            activeObjects = new Dictionary<int, T>(size);
            CurrentObject = obj;
            isReady = true;
        }

        public AssetContainer(string objectKey, Dictionary<int, AssetContainer> objectToContainer, bool forceRelease, int size = 1) : base(objectKey, objectToContainer)
        {
            needforceRelease = forceRelease;
            activeObjects = new Dictionary<int, T>(size);
        }

        public bool RegisterObject(T obj)
        {
            if (activeObjects.TryAdd(obj.GetInstanceID(), obj))
            {
                counter++;
                return true;
            }

            return false;
        }

        /// <summary>
        /// we should release every object b
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="forceRelease"></param>
        /// <returns></returns>
        public override bool ReleaseObject(UnityEngine.Object obj, bool forceRelease = false)
        {
            if (activeObjects.Remove(obj.GetInstanceID()))
            {
                --counter;
                UnityEngine.Object.Destroy(obj);
                TryToRelease(forceRelease);
                return true;
            }

            return false;
        }

        private bool TryToRelease(bool forceRelease)
        {
            if (counter <= 0)
            {
                if (!needforceRelease)
                {
                    Dispose();
                    return true;
                }
                else if (forceRelease)
                {
                    Dispose();
                    return true;
                }
            }

            return false;
        }

        public override void LockContainer()
        {
            needforceRelease = true;
        }

        public override void UnlockForceRelease()
        {
            needforceRelease = false;
        }

        public override void UpdateResourceStatus(Dictionary<int, AssetContainer> objectToContainers, bool forceRelease = false)
        {
            if (!isReady)
                return;

            var activeObjectsCount = activeObjects.Count;
            Span<int> hash = activeObjectsCount < 256 ? stackalloc int[activeObjectsCount] : new int[activeObjectsCount];

            var invalidObjectsCount = 0;

            foreach (var activeObject in activeObjects)
            {
                if (activeObject.Value == null)
                {
                    hash[invalidObjectsCount] = activeObject.Key;
                    objectToContainers.Remove(activeObject.Key);
                    invalidObjectsCount++;
                    counter--;
                }
            }

            if (invalidObjectsCount > 0)
            {
                for (int i = 0; i < invalidObjectsCount; i++)
                {
                    activeObjects.Remove(hash[i]);
                }
            }

            TryToRelease(forceRelease);
        }

        public async UniTask<T> GetAsset(string resourceName, Dictionary<int, AssetContainer> objectToContainer, HashSet<AssetContainer> assetsContainers, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default)
        {
            ++counter;

            while (!isReady)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    --counter;
                    throw new Exception("[AssetService] GetAssetInstance canceled: " + resourceName);
                }

                progress?.Invoke(fillProgress);
                await UniTask.DelayFrame(1);
            }

            if (IsReleased)
                throw new Exception("[AssetService] GetAssetInstance canceled: " + resourceName);

            --counter;
            progress?.Invoke(1f);
            return CurrentObject;
        }

        public async UniTask<T> LoadAsset(string resourceName, Dictionary<int, AssetContainer> objectToContainer, HashSet<AssetContainer> assetsContainers, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var task = Addressables.LoadAssetAsync<T>(resourceName);

                while (!task.IsDone)
                {
                    fillProgress = task.PercentComplete;
                    progress?.Invoke(fillProgress);
                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                }

                if (task.Status == AsyncOperationStatus.Failed)
                    throw new Exception("[AssetService] cant load asset with ID " + resourceName);

                if (cancellationToken.IsCancellationRequested)
                {
                    Dispose();
                    throw new Exception("[AssetService] loading resource canceled: " + resourceName);
                }

                isReady = true;
                CurrentObject = task.Result;
                ObjectID = task.Result.GetInstanceID();
                objectToContainer[ObjectID] = this;
                assetsContainers.Add(this);
                return task.Result;
            }
            catch (Exception ex)
            {
                Debug.LogError("[AssetService] addressable load failed " + ex.ToString() + $" {typeof(T).Name}");
                Dispose();
            }

            throw new Exception("[AssetService] we dont have addressable for " + resourceName);
        }

        public async UniTask<T> GetAsset(AssetReference resourceName, Dictionary<int, AssetContainer> objectToContainer, HashSet<AssetContainer> assetsContainers, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default)
        {
            ++counter;

            while (!isReady)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    --counter;
                    throw new Exception("[AssetService] GetAssetInstance canceled: " + resourceName);
                }

                progress?.Invoke(fillProgress);
                await UniTask.DelayFrame(1);
            }

            if (IsReleased)
                throw new Exception("[AssetService] GetAssetInstance canceled: " + resourceName);

            --counter;
            progress?.Invoke(1f);
            return CurrentObject;
        }

        public async UniTask<T> LoadAsset(AssetReference assetRef, Dictionary<int, AssetContainer> objectToContainer, HashSet<AssetContainer> assetsContainers, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default)
        {
            try
            {
                var task = Addressables.LoadAssetAsync<T>(assetRef);

                while (!task.IsDone)
                {
                    fillProgress = task.PercentComplete;
                    progress?.Invoke(fillProgress);
                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                }

                if (task.Status == AsyncOperationStatus.Failed)
                    throw new Exception("[AssetService] cant load asset with ID " + assetRef);

                if (cancellationToken.IsCancellationRequested)
                {
                    Dispose();
                    throw new Exception("[AssetService] loading resource canceled: " + assetRef);
                }

                isReady = true;
                CurrentObject = task.Result;
                ObjectID = task.Result.GetInstanceID();
                objectToContainer[ObjectID] = this;
                assetsContainers.Add(this);
                return task.Result;
            }
            catch (Exception ex)
            {
                Debug.LogError("[AssetService] addressable load failed " + ex.ToString() + $" {typeof(T).Name}");
                Dispose();
            }

            throw new Exception("[AssetService] we dont have addressable for " + assetRef);
        }



        public override void Dispose()
        {
            if (isDisposed)
                return;

            objectToContainer.Remove(ObjectID);

            isDisposed = true;
            isReady = true;
            IsReleased = true;
            CurrentObject = null;

            AssetContainerHolder<T>.KeyToAssetContainer.Remove(ObjectKey);

            if (CurrentObject != null)
                Addressables.Release(CurrentObject);
        }
    }

    public static class AssetServiceHelpers
    {
        private static string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static string FormatBytes(this long byteCount)
        {
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{Math.Sign(byteCount) * num:F1} {suf[place]}";
        }
    }
    #endregion
}