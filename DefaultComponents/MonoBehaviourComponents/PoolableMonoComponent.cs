using HECSFramework.Core;
using Systems;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Documentation(Doc.GameLogic, Doc.Visual, "Основной компонент для пулинга вьюшек, добаляем на нужный объект, добавляем ассет реф, и не забываем что вьюшку нужно релизить после того как попользовались в " + nameof(PoolingSystem))]
public class PoolableMonoComponent : MonoBehaviour, IPoolableView
{
    public AssetReference AssetReference;

    public string AddressableKey => AssetReference.AssetGUID;

    public GameObject View => gameObject;

    public void Stop()
    {
        var needForStop = GetComponentsInChildren<IStopOnPooling>();

        foreach (var needed in needForStop)
            needed.Stop();
    }

    public void Start()
    {
        var needForStart = GetComponentsInChildren<IStartOnPooling>();

        foreach (var needed in needForStart)
            needed.Start();
    }
}

public interface IStopOnPooling
{
    void Stop();
}

public interface IStartOnPooling
{
    void Start();
}

public interface IPoolableView
{
    string AddressableKey { get; }
    GameObject View { get; }
    void Stop();
    void Start();
}