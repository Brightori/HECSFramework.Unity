using Components;
using HECSFramework.Core;
using HECSFramework.Documentation;
using UnityEngine.InputSystem;

namespace Commands
{
    [Documentation(Doc.Input, "Обозначает нажатие на кнопку (или что-то другое в зависимости от конфигурации InputAction)")]
    public struct InputStartedCommand : IGlobalCommand
    {
        public InputAction.CallbackContext Context { get; set; }
    }

    [Documentation(Doc.Input, "Обозначает отпускание кнопки (или что-то другое в зависимости от конфигурации InputAction)")]
    public struct InputEndedCommand : IGlobalCommand
    {
        public InputAction.CallbackContext Context { get; set; }
    }

    [Documentation(Doc.Input, "Обозначает удерживание кнопки (или что-то другое в зависимости от конфигурации InputAction)")]
    public struct InputCommand : IGlobalCommand
    {
        public InputAction.CallbackContext Context { get; set; }
    }
}