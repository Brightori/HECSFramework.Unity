using Cysharp.Threading.Tasks;
using HECSFramework.Core;
using Helpers;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly Dictionary<AssetKey, AssetContainer> keyToContainer = new Dictionary<AssetKey, AssetContainer>(32);
        private readonly Queue<AssetContainer> releasedContainers = new Queue<AssetContainer>(32);

        public override void InitSystem()
        {
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;

            foreach (var container in assetsContainers)
                container.Dispose();

            assetsContainers.Clear();
            objectToContainer.Clear();
            keyToContainer.Clear();
            releasedContainers.Clear();
            base.Dispose();
        }

        #region Containers

        private AssetContainer<T> GetOrCreateContainer<T>(AssetKey key, bool isForceRelease, out bool created) where T : UnityEngine.Object
        {
            if (keyToContainer.TryGetValue(key, out var existing) && !existing.IsReleased)
            {
                var typed = (AssetContainer<T>)existing;

                if (isForceRelease)
                    typed.LockContainer();

                created = false;
                return typed;
            }

            var container = new AssetContainer<T>(key, objectToContainer, keyToContainer, assetsContainers, isForceRelease);
            keyToContainer[key] = container;
            created = true;
            return container;
        }

        private async UniTask<AssetContainer<T>> LoadContainer<T>(AssetKey key, object addressableKey, ProgressUpdate progress, bool isForceRelease, CancellationToken cancellationToken) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            Debug.Log($"[AssetService] GetAsset {key}");
#endif
            var container = GetOrCreateContainer<T>(key, isForceRelease, out var created);

            if (created)
                await container.LoadAsset(addressableKey, progress, cancellationToken);
            else
                await container.WaitAsset(progress, cancellationToken);

            return container;
        }

        private AssetContainer<T> LoadContainerSync<T>(AssetKey key, object addressableKey, bool isForceRelease) where T : UnityEngine.Object
        {
#if UNITY_EDITOR
            Debug.Log($"[AssetService] GetAssetSync {key}");
#endif
            var container = GetOrCreateContainer<T>(key, isForceRelease, out var created);

            if (created)
                container.LoadAssetSync(addressableKey);
            else
                container.WaitAssetSync();

            return container;
        }

        public bool TryGetContainer<T>(string resourceName, out AssetContainer<T> container) where T : UnityEngine.Object
            => TryGetContainer(AssetKey.Address<T>(resourceName), out container);

        public bool TryGetContainer<T>(AssetReference assetReference, out AssetContainer<T> container) where T : UnityEngine.Object
            => TryGetContainer(AssetKey.Reference<T>(assetReference), out container);

        private bool TryGetContainer<T>(AssetKey key, out AssetContainer<T> container) where T : UnityEngine.Object
        {
            if (keyToContainer.TryGetValue(key, out var existing) && !existing.IsReleased)
            {
                container = (AssetContainer<T>)existing;
                return true;
            }

            container = null;
            return false;
        }

        private void RegisterInstance<T>(AssetContainer<T> container, T instance) where T : UnityEngine.Object
        {
            if (container.RegisterObject(instance))
                objectToContainer[instance.GetAdaptedInstanceID()] = container;
        }

        private static Quaternion SafeRotation(Quaternion rotation)
            => rotation.x == 0f && rotation.y == 0f && rotation.z == 0f && rotation.w == 0f ? Quaternion.identity : rotation;

        #endregion

        #region GetAsset

        public async UniTask<T> GetAsset<T>(string resourceName, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("[AssetService] GetAsset invalid name provided", nameof(resourceName));

            var container = await LoadContainer<T>(AssetKey.Address<T>(resourceName), resourceName, progress, isForceRelease, cancellationToken);
            return container.CurrentObject;
        }

        public async UniTask<T> GetAsset<T>(AssetReference assetReference, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (assetReference == null || !assetReference.RuntimeKeyIsValid())
                throw new ArgumentException("[AssetService] GetAsset invalid asset reference provided", nameof(assetReference));

            var container = await LoadContainer<T>(AssetKey.Reference<T>(assetReference), assetReference.RuntimeKey, progress, isForceRelease, cancellationToken);
            return container.CurrentObject;
        }

        public async UniTask<(bool, T)> TryGetAsset<T>(string resourceName, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("[AssetService] TryGetAsset invalid name provided", nameof(resourceName));

            if (TryGetContainer<T>(resourceName, out var ready))
                return (true, await ready.WaitAsset(progress, cancellationToken));

            var locations = await LoadLocations(resourceName, typeof(T));

            if (locations.Count == 0)
                return (false, null);

            return (true, await GetAsset<T>(resourceName, progress, isForceRelease, cancellationToken));
        }

        public async UniTask<IList<T>> GetAssetsByTag<T>(string label, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var locations = await LoadLocations(label, typeof(T));
            var list = new List<UniTask<T>>(locations.Count);

            foreach (var location in locations)
                list.Add(GetAsset<T>(location.PrimaryKey, null, isForceRelease, cancellationToken));

            return await UniTask.WhenAll(list);
        }

        public T GetAssetSync<T>(string resourceName, bool isForceRelease = false) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("[AssetService] GetAssetSync invalid name provided", nameof(resourceName));

            return LoadContainerSync<T>(AssetKey.Address<T>(resourceName), resourceName, isForceRelease).CurrentObject;
        }

        #endregion

        #region GetComponentFromAsset

        public T GetComponentFromAssetSync<T>(string resourceName, bool isForceRelease = false) where T : Component
        {
            return GetAssetSync<GameObject>(resourceName, isForceRelease).GetComponent<T>();
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
            var instance = await GetAssetInstance(resourceName, pos, rotation, parent, progress, newAsyncInstatiate, cancellationToken);
            return instance.GetComponent<T>();
        }

        public T GetComponentFromGameObjectInstanceSync<T>(string resourceName, Transform parent = null, Vector3 pos = default, Quaternion rotation = default) where T : Component
        {
            return GetAssetInstanceSync<GameObject>(resourceName, parent, pos, rotation).GetComponent<T>();
        }

        #endregion

        #region GetAssetInstance

        public async UniTask<GameObject> GetAssetInstance(string resourceName, CancellationToken cancellationToken = default)
        {
            var container = await LoadContainer<GameObject>(AssetKey.Address<GameObject>(resourceName), resourceName, null, false, cancellationToken);
            var result = UnityEngine.Object.Instantiate(container.CurrentObject);
            RegisterInstance(container, result);
            return result;
        }

        public async UniTask<GameObject> GetAssetInstance(string resourceName, Transform parent, CancellationToken cancellationToken = default)
        {
            var container = await LoadContainer<GameObject>(AssetKey.Address<GameObject>(resourceName), resourceName, null, false, cancellationToken);
            var result = UnityEngine.Object.Instantiate(container.CurrentObject, parent);
            RegisterInstance(container, result);
            return result;
        }

        public async UniTask<GameObject> GetAssetInstance(string resourceName, Vector3 pos, Quaternion rotation, Transform parent = null, ProgressUpdate progress = null, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default)
        {
            var container = await LoadContainer<GameObject>(AssetKey.Address<GameObject>(resourceName), resourceName, progress, false, cancellationToken);
            return await InstantiateOne(container, pos, rotation, parent, progress, newAsyncInstatiate, cancellationToken);
        }

        public async UniTask<GameObject> GetAssetInstance(AssetReference assetReference, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default)
        {
            return await GetAssetInstance(assetReference, Vector3.zero, Quaternion.identity, null, null, newAsyncInstatiate, cancellationToken);
        }

        public async UniTask<GameObject> GetAssetInstance(AssetReference assetReference, Vector3 pos, Quaternion rotation, Transform parent = null, ProgressUpdate progress = null, bool newAsyncInstatiate = false, CancellationToken cancellationToken = default)
        {
            var container = await LoadContainer<GameObject>(AssetKey.Reference<GameObject>(assetReference), assetReference.RuntimeKey, progress, false, cancellationToken);
            return await InstantiateOne(container, pos, rotation, parent, progress, newAsyncInstatiate, cancellationToken);
        }

        public async UniTask<GameObject[]> GetAssetsInstance(AssetReference assetReference, Vector3 pos, Quaternion rotation, Transform parent = null, ProgressUpdate progress = null, CancellationToken cancellationToken = default, int neededCount = 1)
        {
            if (neededCount <= 0)
                return Array.Empty<GameObject>();

            var container = await LoadContainer<GameObject>(AssetKey.Reference<GameObject>(assetReference), assetReference.RuntimeKey, progress, false, cancellationToken);
            var result = await InstantiateAsync(container.CurrentObject, neededCount, parent, pos, SafeRotation(rotation), progress, cancellationToken);

            foreach (var instance in result)
                RegisterInstance(container, instance);

            return result;
        }

        private async UniTask<GameObject> InstantiateOne(AssetContainer<GameObject> container, Vector3 pos, Quaternion rotation, Transform parent, ProgressUpdate progress, bool newAsyncInstatiate, CancellationToken cancellationToken)
        {
            rotation = SafeRotation(rotation);
            GameObject result;

            if (newAsyncInstatiate)
                result = (await InstantiateAsync(container.CurrentObject, 1, parent, pos, rotation, progress, cancellationToken))[0];
            else
                result = UnityEngine.Object.Instantiate(container.CurrentObject, pos, rotation, parent);

            RegisterInstance(container, result);
            return result;
        }

        private static async UniTask<GameObject[]> InstantiateAsync(GameObject prefab, int count, Transform parent, Vector3 pos, Quaternion rotation, ProgressUpdate progress, CancellationToken cancellationToken)
        {
            var operation = UnityEngine.Object.InstantiateAsync(prefab, count, parent, pos, rotation);

            while (!operation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    operation.Cancel();
                    DestroyAll(operation);
                    throw new OperationCanceledException(cancellationToken);
                }

                progress?.Invoke(operation.progress);
                await UniTask.Yield();
            }

            if (cancellationToken.IsCancellationRequested)
            {
                DestroyAll(operation);
                throw new OperationCanceledException(cancellationToken);
            }

            return operation.Result;
        }

        private static void DestroyAll(AsyncInstantiateOperation<GameObject> operation)
        {
            if (!operation.isDone || operation.Result == null)
                return;

            foreach (var instance in operation.Result)
            {
                if (instance != null)
                    UnityEngine.Object.Destroy(instance);
            }
        }

        public GameObject GetAssetInstanceSync(string resourceName)
        {
            return GetAssetInstanceSync<GameObject>(resourceName, null);
        }

        public T GetAssetInstanceSync<T>(string resourceName, Transform parent) where T : UnityEngine.Object
        {
            var container = LoadContainerSync<T>(AssetKey.Address<T>(resourceName), resourceName, false);
            var result = UnityEngine.Object.Instantiate(container.CurrentObject, parent);
            RegisterInstance(container, result);
            return result;
        }

        public T GetAssetInstanceSync<T>(string resourceName, Transform parent = null, Vector3 pos = default, Quaternion rotation = default) where T : UnityEngine.Object
        {
            var container = LoadContainerSync<T>(AssetKey.Address<T>(resourceName), resourceName, false);
            var result = UnityEngine.Object.Instantiate(container.CurrentObject, pos, SafeRotation(rotation), parent);
            RegisterInstance(container, result);
            return result;
        }

        #endregion

        #region WarmUp

        public UniTask WarmUpAssetsProgress<T>(string bundleNameOrTag, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
            => WarmUpAssets<T>(bundleNameOrTag, progress, isForceRelease, cancellationToken);

        public UniTask WarmUpAssetsProgress<T>(IEnumerable<string> resources, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
            => WarmUpAssets<T>(resources, progress, isForceRelease, cancellationToken);

        public async UniTask WarmUpAssets<T>(string bundleNameOrTag, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var locations = await LoadLocations(bundleNameOrTag, typeof(T));
            await ProcessWarmUpAssets<T>(locations, progress, isForceRelease, cancellationToken);
        }

        public async UniTask WarmUpAssets<T>(IEnumerable<string> resources, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var locations = await LoadLocationsMany(resources, typeof(T));
            await ProcessWarmUpAssets<T>(locations, progress, isForceRelease, cancellationToken);
        }

        public async UniTask WarmUpAsset<T>(AssetReference assetReference, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            await GetAsset<T>(assetReference, progress, isForceRelease, cancellationToken);
        }

        public async UniTask WarmUpAssets<T>(AssetReference[] assetReferences, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var count = assetReferences.Length;

            if (count == 0)
            {
                progress?.Invoke(1f);
                return;
            }

            var tasks = new List<UniTask>(count);
            var aggregator = progress != null ? new ProgressAggregator(count, progress) : null;

            for (int i = 0; i < count; i++)
                tasks.Add(GetAsset<T>(assetReferences[i], aggregator?.Part(i), isForceRelease, cancellationToken));

            await UniTask.WhenAll(tasks);
        }

        public async UniTask WarmUpAssets<T>(List<AssetReference> assetReferences, ProgressUpdate progress = null, bool isForceRelease = false, CancellationToken cancellationToken = default) where T : UnityEngine.Object
        {
            var count = assetReferences.Count;

            if (count == 0)
            {
                progress?.Invoke(1f);
                return;
            }

            var tasks = new List<UniTask>(count);
            var aggregator = progress != null ? new ProgressAggregator(count, progress) : null;

            for (int i = 0; i < count; i++)
                tasks.Add(GetAsset<T>(assetReferences[i], aggregator?.Part(i), isForceRelease, cancellationToken));

            await UniTask.WhenAll(tasks);
        }

        private async UniTask ProcessWarmUpAssets<T>(IList<IResourceLocation> locations, ProgressUpdate progress, bool isForceRelease, CancellationToken cancellationToken) where T : UnityEngine.Object
        {
            var count = locations.Count;

            if (count == 0)
            {
                progress?.Invoke(1f);
                return;
            }

            var tasks = new List<UniTask>(count);
            var aggregator = progress != null ? new ProgressAggregator(count, progress) : null;

            for (int i = 0; i < count; i++)
                tasks.Add(GetAsset<T>(locations[i].PrimaryKey, aggregator?.Part(i), isForceRelease, cancellationToken));

            await UniTask.WhenAll(tasks);
        }

        #endregion

        #region Locations

        private static async UniTask<IList<IResourceLocation>> LoadLocations(object key, Type type = null)
        {
            var handle = Addressables.LoadResourceLocationsAsync(key, type);

            try
            {
                var result = await handle.Task;
                return new List<IResourceLocation>(result);
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        private static async UniTask<IList<IResourceLocation>> LoadLocationsMany(IEnumerable keys, Type type = null)
        {
            var handle = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union, type);

            try
            {
                var result = await handle.Task;
                return new List<IResourceLocation>(result);
            }
            finally
            {
                Addressables.Release(handle);
            }
        }

        #endregion

        #region LoadRemote

        public async UniTask LoadRemote(string groupNameOrTag, ProgressUpdate progress, DownloadSizeUpdate downloadSizeUpdate)
        {
            try
            {
                var handle = Addressables.DownloadDependenciesAsync(groupNameOrTag);

                try
                {
                    while (!handle.IsDone)
                    {
                        await UniTask.Yield();

                        var status = handle.GetDownloadStatus();
                        progress?.Invoke(status.Percent);
                        downloadSizeUpdate?.Invoke(status.DownloadedBytes, status.TotalBytes);
#if UNITY_EDITOR
                        Debug.Log($"[BundleLoadingProcess] '{groupNameOrTag}' Loading..., Progress: {status.DownloadedBytes}/{status.TotalBytes} ({status.DownloadedBytes.FormatBytes()}/{status.TotalBytes.FormatBytes()})");
#endif
                    }

                    if (handle.Status == AsyncOperationStatus.Failed)
                    {
                        Debug.LogError($"[AssetService] Downloading error for '{groupNameOrTag}': {GetDownloadError(handle)}");
                    }
                    else
                    {
                        var status = handle.GetDownloadStatus();

                        if (status.DownloadedBytes < status.TotalBytes)
                            Debug.LogError($"[AssetService] Bundle '{groupNameOrTag}' finished, but downloaded less than needed: {status.DownloadedBytes}/{status.TotalBytes}");
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
            var resourceList = new List<string>(resources);
            var count = resourceList.Count;

            if (count == 0)
            {
                progress?.Invoke(1f);
                return;
            }

            var locations = await LoadLocationsMany(resourceList);

            var sizeHandle = Addressables.GetDownloadSizeAsync(locations);
            long overallSize;

            try
            {
                overallSize = await sizeHandle.Task;
            }
            finally
            {
                Addressables.Release(sizeHandle);
            }

#if UNITY_EDITOR
            Debug.LogWarning($"[AssetService] DownLoadSize of all dependencies is {overallSize}");
#endif

            var tasks = new List<UniTask>(count);
            var aggregator = progress != null ? new ProgressAggregator(count, progress) : null;
            var downloaded = new long[count];
            long downloadedSum = 0;

            for (int i = 0; i < count; i++)
            {
                var index = i;
                DownloadSizeUpdate sizeUpdate = downloadSizeUpdate == null ? null : (current, _) =>
                {
                    downloadedSum += current - downloaded[index];
                    downloaded[index] = current;
                    downloadSizeUpdate.Invoke(downloadedSum, overallSize);
                };

                tasks.Add(LoadRemote(resourceList[i], aggregator?.Part(i), sizeUpdate));
            }

            try
            {
                await UniTask.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Debug.LogError("[AssetService] remote load failed " + ex);
                throw;
            }
        }

        private static string GetDownloadError(AsyncOperationHandle fromHandle)
        {
            if (fromHandle.Status != AsyncOperationStatus.Failed)
                return null;

            var e = fromHandle.OperationException;

            while (e != null)
            {
                if (e is RemoteProviderException remoteException)
                    return remoteException.WebRequestResult.Error;

                e = e.InnerException;
            }

            return fromHandle.OperationException?.Message;
        }

        #endregion

        #region ReleaseAndUnload

        public bool Release<T>(T obj, bool force = false) where T : UnityEngine.Object
        {
            if (obj == null)
                return false;

            var id = obj.GetAdaptedInstanceID();

            if (objectToContainer.TryGetValue(id, out var container))
            {
                if (container.ObjectID == id)
                {
                    container.UpdateResourceStatus(force);
                    return container.IsReleased;
                }

                objectToContainer.Remove(id);
                return container.ReleaseObject(obj, force);
            }

            if (obj is GameObject gameObject && gameObject.scene.IsValid())
            {
                UnityEngine.Object.Destroy(gameObject);
                return false;
            }

            Debug.LogWarning($"[AssetService] Release called for untracked object {obj.name} ({typeof(T).Name}), nothing done");
            return false;
        }

        public void UnloadUnusedResources(bool forceRelease = false)
        {
            foreach (var container in assetsContainers)
            {
                container.UpdateResourceStatus(forceRelease);

                if (container.IsReleased)
                    releasedContainers.Enqueue(container);
            }

            while (releasedContainers.TryDequeue(out var container))
                assetsContainers.Remove(container);
        }

        public async UniTask ReleaseByTag(string tag)
        {
            var locations = await LoadLocations(tag);

            foreach (var location in locations)
            {
                foreach (var container in assetsContainers)
                {
                    if (container.Key.ByReference || container.Key.Key != location.PrimaryKey)
                        continue;

                    container.UnlockForceRelease();
                    container.UpdateResourceStatus();

                    if (container.IsReleased)
                        releasedContainers.Enqueue(container);
                }
            }

            while (releasedContainers.TryDequeue(out var container))
                assetsContainers.Remove(container);
        }

        #endregion
    }

    #region AssetContainerAndHelpers

    public readonly struct AssetKey : IEquatable<AssetKey>
    {
        public readonly string Key;
        public readonly Type Type;
        public readonly bool ByReference;

        private AssetKey(string key, Type type, bool byReference)
        {
            Key = key;
            Type = type;
            ByReference = byReference;
        }

        public static AssetKey Address<T>(string address) => new AssetKey(address, typeof(T), false);

        public static AssetKey Reference<T>(AssetReference reference) => new AssetKey(reference.RuntimeKey.ToString(), typeof(T), true);

        public bool Equals(AssetKey other) => ByReference == other.ByReference && Type == other.Type && Key == other.Key;
        public override bool Equals(object obj) => obj is AssetKey other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Key, Type, ByReference);
        public override string ToString() => Key;
    }

    public sealed class ProgressAggregator
    {
        private readonly float[] parts;
        private readonly ProgressUpdate target;
        private float sum;

        public ProgressAggregator(int count, ProgressUpdate target)
        {
            parts = new float[count];
            this.target = target;
        }

        public ProgressUpdate Part(int index) => value =>
        {
            sum += value - parts[index];
            parts[index] = value;
            target.Invoke(sum / parts.Length);
        };
    }

    public abstract class AssetContainer : IDisposable
    {
        public int ObjectID { get; protected set; }
        public readonly AssetKey Key;
        public string ObjectKey => Key.Key;

        protected readonly Dictionary<int, AssetContainer> objectToContainer;
        protected readonly Dictionary<AssetKey, AssetContainer> keyToContainer;
        protected readonly HashSet<AssetContainer> assetsContainers;

        [ShowInInspector]
        protected int counter;

        public abstract bool IsReady { get; }
        public bool IsReleased { get; protected set; }

        protected AssetContainer(AssetKey key, Dictionary<int, AssetContainer> objectToContainer, Dictionary<AssetKey, AssetContainer> keyToContainer, HashSet<AssetContainer> assetsContainers)
        {
            Key = key;
            this.objectToContainer = objectToContainer;
            this.keyToContainer = keyToContainer;
            this.assetsContainers = assetsContainers;
        }

        public abstract bool ReleaseObject(UnityEngine.Object obj, bool forceRelease = false);
        public abstract void LockContainer();
        public abstract void UnlockForceRelease();
        public abstract void UpdateResourceStatus(bool forceRelease = false);
        public abstract void Dispose();
    }

    public class AssetContainer<T> : AssetContainer where T : UnityEngine.Object
    {
        private readonly Dictionary<int, T> activeObjects;
        private AsyncOperationHandle<T> handle;

        private bool needForceRelease;
        private bool isDisposed;
        private bool isReady;
        private float fillProgress;

        [ShowInInspector]
        public T CurrentObject { get; private set; }
        public override bool IsReady => isReady;

        public AssetContainer(AssetKey key, Dictionary<int, AssetContainer> objectToContainer, Dictionary<AssetKey, AssetContainer> keyToContainer, HashSet<AssetContainer> assetsContainers, bool forceRelease, int size = 1)
            : base(key, objectToContainer, keyToContainer, assetsContainers)
        {
            needForceRelease = forceRelease;
            activeObjects = new Dictionary<int, T>(size);
        }

        #region Loading

        public async UniTask<T> LoadAsset(object addressableKey, ProgressUpdate progress = null, CancellationToken cancellationToken = default)
        {
            ++counter;

            try
            {
                handle = Addressables.LoadAssetAsync<T>(addressableKey);
                return await WaitInternal(progress, cancellationToken);
            }
            finally
            {
                --counter;
            }
        }

        public async UniTask<T> WaitAsset(ProgressUpdate progress = null, CancellationToken cancellationToken = default)
        {
            ++counter;

            try
            {
                return await WaitInternal(progress, cancellationToken);
            }
            finally
            {
                --counter;
            }
        }

        private async UniTask<T> WaitInternal(ProgressUpdate progress, CancellationToken cancellationToken)
        {
            try
            {
                while (!isReady)
                {
                    if (isDisposed)
                        throw new Exception("[AssetService] container was released while loading " + Key);

                    if (handle.IsValid() && handle.IsDone)
                    {
                        CompleteFromHandle();
                        break;
                    }

                    fillProgress = handle.IsValid() ? handle.PercentComplete : 0f;
                    progress?.Invoke(fillProgress);
                    await UniTask.DelayFrame(1, cancellationToken: cancellationToken);
                }

                progress?.Invoke(1f);
                return CurrentObject;
            }
            catch (OperationCanceledException)
            {
                if (counter <= 1)
                    Dispose();

                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetService] addressable load failed for {Key} ({typeof(T).Name}): {ex}");
                Dispose();
                throw;
            }
        }

        public T LoadAssetSync(object addressableKey)
        {
            handle = Addressables.LoadAssetAsync<T>(addressableKey);
            return WaitAssetSync();
        }

        public T WaitAssetSync()
        {
            if (isReady)
                return CurrentObject;

            if (isDisposed || !handle.IsValid())
                throw new Exception("[AssetService] container has no pending load for " + Key);

            try
            {
                handle.WaitForCompletion();
                CompleteFromHandle();
                return CurrentObject;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AssetService] addressable sync load failed for {Key} ({typeof(T).Name}): {ex}");
                Dispose();
                throw;
            }
        }

        private void CompleteFromHandle()
        {
            if (isReady || isDisposed)
                return;

            if (handle.Status != AsyncOperationStatus.Succeeded || handle.Result == null)
                throw new Exception($"[AssetService] cant load asset {Key}: {handle.OperationException?.Message}");

            CurrentObject = handle.Result;
            ObjectID = CurrentObject.GetAdaptedInstanceID();
            isReady = true;
            fillProgress = 1f;
            objectToContainer[ObjectID] = this;
            assetsContainers.Add(this);
        }

        #endregion

        #region Instances

        public bool RegisterObject(T obj)
        {
            if (activeObjects.TryAdd(obj.GetAdaptedInstanceID(), obj))
            {
                counter++;
                return true;
            }

            return false;
        }

        public override bool ReleaseObject(UnityEngine.Object obj, bool forceRelease = false)
        {
            if (activeObjects.Remove(obj.GetAdaptedInstanceID()))
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
            if (counter > 0)
                return false;

            if (needForceRelease && !forceRelease)
                return false;

            Dispose();
            return true;
        }

        public override void LockContainer()
        {
            needForceRelease = true;
        }

        public override void UnlockForceRelease()
        {
            needForceRelease = false;
        }

        public override void UpdateResourceStatus(bool forceRelease = false)
        {
            if (!isReady || isDisposed)
                return;

            var activeObjectsCount = activeObjects.Count;
            Span<int> invalid = activeObjectsCount < 256 ? stackalloc int[activeObjectsCount] : new int[activeObjectsCount];
            var invalidCount = 0;

            foreach (var activeObject in activeObjects)
            {
                if (activeObject.Value == null)
                {
                    invalid[invalidCount++] = activeObject.Key;
                    objectToContainer.Remove(activeObject.Key);
                    counter--;
                }
            }

            for (int i = 0; i < invalidCount; i++)
                activeObjects.Remove(invalid[i]);

            TryToRelease(forceRelease);
        }

        #endregion

        public override void Dispose()
        {
            if (isDisposed)
                return;

            isDisposed = true;
            IsReleased = true;
            isReady = true;

            if (objectToContainer.TryGetValue(ObjectID, out var byObject) && ReferenceEquals(byObject, this))
                objectToContainer.Remove(ObjectID);

            if (keyToContainer.TryGetValue(Key, out var byKey) && ReferenceEquals(byKey, this))
                keyToContainer.Remove(Key);

            activeObjects.Clear();
            CurrentObject = null;

            if (handle.IsValid())
                Addressables.Release(handle);

            handle = default;
        }
    }



    public static class AssetServiceHelpers
    {
        private static readonly string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

        public static string FormatBytes(this long byteCount)
        {
            if (byteCount == 0)
                return "0" + suffixes[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{Math.Sign(byteCount) * num:F1} {suffixes[place]}";
        }
    }

    #endregion
}