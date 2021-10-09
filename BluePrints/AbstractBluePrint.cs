using UnityEngine;

namespace HECSFramework.Unity
{
    public class AbstractBluePrint<T> : ScriptableObject
    {
        [SerializeField] T GetData = default;
    }
}