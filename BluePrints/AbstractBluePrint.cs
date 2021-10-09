using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class AbstractBluePrint<T> : ScriptableObject
    {
        [SerializeField] protected T data = default;
        public abstract T GetData { get; }
    }
}