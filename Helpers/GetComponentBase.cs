using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;


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
            Assert.IsNotNull(component, $"нет компонента Button у {gameObject.name}");

            return component;
        }
    }
}