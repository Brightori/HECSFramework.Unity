using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Components.MonoBehaviourComponents
{
    public class DragJoystickControl : OnScreenControl
    {
        [field: InputControl(layout = "Vector2"), SerializeField]
        protected override string controlPathInternal { get; set; }

        private Joystick joystick;

        private void Awake()
        {
            joystick = GetComponentInChildren<Joystick>(true);
            Assert.IsNotNull(joystick);
        }

        public void Update()
        {
            Signal(new Vector2(joystick.Horizontal, joystick.Vertical));
        }
        private void Signal(Vector2 value)
        {
            if (!string.IsNullOrEmpty(controlPathInternal))
                SendValueToControl(value);
        }
    }
}