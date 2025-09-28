using HECSFramework.Core;
using Systems;
using UnityEngine;

[Documentation(Doc.GameLogic, Doc.Visual, Doc.Poolable, "Main monobeh component for tagging poolable view" + nameof(PoolingSystem))]
public class PoolableMonoComponent : MonoBehaviour, IPoolableView
{
    public GameObject View => gameObject;

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
    GameObject View { get; }
    void Stop();
    void Start();
}