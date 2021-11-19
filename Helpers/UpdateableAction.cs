using System;
using UnityEngine.InputSystem;

namespace Components
{
    /// <summary>
    /// Хелпер для более удобной обработки InputAction'ов
    /// </summary>
    public class UpdateableAction : IDisposable
    {
        private readonly InputAction move;

        private InputAction.CallbackContext cachedForUpdate;
        private bool isPressed;

        public event Action<InputAction.CallbackContext> OnStart;
        public event Action<InputAction.CallbackContext> OnEnd;
        public event Action<InputAction.CallbackContext> OnUpdate;
        
        public void UpdateAction()
        {
            if (!isPressed) return;
            OnUpdate?.Invoke(cachedForUpdate);
        }

        public UpdateableAction(InputAction move)
        {
            this.move = move;
            move.started += Started;
            move.performed += Updated;
            move.canceled += Ended;
        }


        private void Ended(InputAction.CallbackContext obj)
        {
            isPressed = false;
            OnEnd?.Invoke(obj);
        }

        private void Updated(InputAction.CallbackContext obj)
            => cachedForUpdate = obj;

        private void Started(InputAction.CallbackContext obj)
        {
            OnStart?.Invoke(obj);
            isPressed = true;
        }


        public void Dispose()
        {
            move.started -= Started;
            move.performed -= Updated;
            move.canceled -= Ended;
            OnStart = null;
            OnEnd = null;
            OnUpdate = null;
        }
    }
}