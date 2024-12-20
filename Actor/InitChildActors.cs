using HECSFramework.Unity;
using UnityEngine;

[DefaultExecutionOrder(100)]
public class InitChildActors : MonoBehaviour
{
    private void Awake()
    {
        var actor = GetComponent<Actor>();
        var actorInChilds = GetComponentsInChildren<Actor>();

        foreach (var child in actorInChilds)
        {
            if (child == actor)
                continue;

            if (child.IsInited)
                continue;

            child.InitWithContainer();
        }
    }
}