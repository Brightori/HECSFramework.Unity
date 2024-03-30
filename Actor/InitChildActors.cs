using HECSFramework.Unity;
using UnityEngine;

public class InitChildActors : MonoBehaviour
{
    private void Awake()
    {
        var actorInChilds = GetComponentsInChildren<Actor>();

        foreach (var child in actorInChilds)
            child.InitWithContainer();
    }
}