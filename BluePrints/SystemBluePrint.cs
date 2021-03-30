using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class SystemBluePrint<T> : SystemBaseBluePrint where T: ISystem, new ()
    {
        [SerializeField, HideInInspector] private T system = new T();
        public override ISystem GetSystem => system;
    }
}