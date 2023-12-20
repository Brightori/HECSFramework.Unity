using HECSFramework.Core;
using UnityEngine.InputSystem;

namespace Commands
{
    [Documentation(Doc.Input, "Обозначает нажатие на кнопку (или что-то другое в зависимости от конфигурации InputAction)")]
    public struct InputStartedCommand : IGlobalCommand
    {
        public int Index;
        public InputAction.CallbackContext Context;
    }

    [Documentation(Doc.Input, "Обозначает отпускание кнопки (или что-то другое в зависимости от конфигурации InputAction)")]
    public struct InputEndedCommand : IGlobalCommand
    {
        public int Index;
        public InputAction.CallbackContext Context;
    }

    [Documentation(Doc.Input, "Обозначает удерживание кнопки (или что-то другое в зависимости от конфигурации InputAction)")]
    public struct InputCommand : IGlobalCommand
    {
        public int Index;
        public InputAction.CallbackContext Context;
    }
}