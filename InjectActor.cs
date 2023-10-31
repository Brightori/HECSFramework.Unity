using HECSFramework.Unity;
using UnityEngine;

[DisallowMultipleComponent]
[DefaultExecutionOrder(-10)]
public class InjectActor : MonoBehaviour
{
    private void Awake()
    {
        var actor = GetComponent<Actor>();

        var needActor = GetComponentsInChildren<IHaveActor>(true);

        foreach (var na in needActor)
        {
            na.Actor = actor;
        }
    }
}