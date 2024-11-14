using HECSFramework.Core;
using Systems;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Documentation(Doc.GameLogic, Doc.Visual, Doc.Poolable, "Main monobeh component for tagging poolable view" + nameof(PoolingSystem))]
public class PoolableMonoComponent : MonoBehaviour, IPoolableView
{
    public AssetReference AssetReference;

    public string AddressableKey => AssetReference.AssetGUID;

    public GameObject View => gameObject;

    public AssetReference AssetRef => AssetReference;

    void IPoolableView.Stop()
    {
        var needForStop = GetComponentsInChildren<IStopOnPooling>();

        foreach (var needed in needForStop)
            needed.Stop();
    }

    void IPoolableView.Start()
    {
        var needForStart = GetComponentsInChildren<IStartOnPooling>();

        foreach (var needed in needForStart)
            needed.StartOnPooling();
    }
}

public interface IStopOnPooling
{
    void Stop();
}

public interface IStartOnPooling
{
    void StartOnPooling();
}

public interface IPoolableView
{
    string AddressableKey { get; }
    GameObject View { get; }
    AssetReference AssetRef { get; }
    void Stop();
    void Start();
}