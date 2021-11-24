using System;
using UnityEngine.InputSystem;

namespace Components
{
    /// <summary>
    /// Хелпер для более удобной обработки InputAction'ов
    /// </summary>
    public class UpdateableAction : IDisposable
    {
        private readonly InputAction action;

        private InputAction.CallbackContext cachedForUpdate;
        private bool isPressed;
        private int index;

        public event Action<int, InputAction.CallbackContext> OnStart;
        public event Action<int, InputAction.CallbackContext> OnEnd;
        public event Action<int, InputAction.CallbackContext> OnUpdate;
        
        public void UpdateAction()
        {
            if (!isPressed) return;
            OnUpdate?.Invoke(index, cachedForUpdate);
        }

        public UpdateableAction(int index, InputAction action)
        {
            this.action = action;
            action.started += Started;
            action.performed += Updated;
            action.canceled += Ended;
            this.index = index;
        }

        private void Ended(InputAction.CallbackContext obj)
        {
            isPressed = false;
            OnEnd?.Invoke(index,obj);
        }

        private void Updated(InputAction.CallbackContext obj)
        {
            cachedForUpdate = obj;
        }

        private void Started(InputAction.CallbackContext obj)
        {
            OnStart?.Invoke(index, obj);
            isPressed = true;
        }


        public void Dispose()
        {
            action.started -= Started;
            action.performed -= Updated;
            action.canceled -= Ended;
            OnStart = null;
            OnEnd = null;
            OnUpdate = null;
        }
    }
}