using System.Collections.Generic;
using HECSFramework.Core;
using Systems;
using UnityEngine;

[Documentation(Doc.GameLogic, Doc.Visual, Doc.Poolable, "Main monobeh component for tagging poolable view" + nameof(PoolingSystem))]
public class PoolableMonoComponent : MonoBehaviour, IPoolableView
{
    public bool RuntTimeCheck = false;

    private List<IStopOnPooling> needForStop = new List<IStopOnPooling>(2);
    private List<IStartOnPooling> needForStart = new List<IStartOnPooling>(2);

    public GameObject View => gameObject;

    private void Awake()
    {
        GetComponentsInChildren<IStopOnPooling>(needForStop);
        GetComponentsInChildren<IStartOnPooling>(needForStart);
    }

    void IPoolableView.Stop()
    {
        if (RuntTimeCheck)
            GetComponentsInChildren<IStopOnPooling>(needForStop);

        foreach (var needed in needForStop)
            needed.Stop();
    }

    void IPoolableView.Start()
    {
        if (RuntTimeCheck)
            GetComponentsInChildren<IStartOnPooling>(needForStart);

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