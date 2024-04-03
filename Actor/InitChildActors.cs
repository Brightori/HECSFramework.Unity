using HECSFramework.Unity;
using UnityEngine;

public class InitChildActors : MonoBehaviour
{
    private void Awake()
    {
        var actorInChilds = GetComponentsInChildren<Actor>();

        foreach (var child in actorInChilds)
        {
            if (child.IsInited)
                continue;

            child.InitWithContainer();
        }
    }
}