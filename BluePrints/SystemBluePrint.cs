using HECSFramework.Unity;
using UnityEngine;

namespace HECSFrameWork.Systems
{
    public abstract class SystemBluePrint<T> : SystemBaseBluePrint where T: ISystem, new ()
    {
        [SerializeField, HideInInspector] private T system = new T();
        public override ISystem GetSystem => system;
    }
}
