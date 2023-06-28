using UnityEngine;
using UnityEngine.Assertions;


namespace Components
{
    public abstract class GetComponentBase<T> : MonoBehaviour where T : Component
    {
        private T component;

        public T Get()
        {
            if (component != null)
                return component;

            component = GetComponent<T>();
            Assert.IsNotNull(component, $"we dont have component {typeof(T).Name} у {gameObject.name}");

            return component;
        }
    }
}