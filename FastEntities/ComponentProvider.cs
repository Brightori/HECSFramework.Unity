using HECSFramework.Core;
using UnityEngine;

namespace HECSFramework.Unity
{
    public abstract class FastComponentMonoProvider : MonoBehaviour 
    {
        public abstract void AddComponent(FastEntity fastEntity);
    }

    public class FastComponentMonoProvider<T> : FastComponentMonoProvider where T :struct, IFastComponent
    {
        public T Component;

        public override void AddComponent(FastEntity fastEntity)
        {
            fastEntity.AddComponent(Component);
        }
    }
}