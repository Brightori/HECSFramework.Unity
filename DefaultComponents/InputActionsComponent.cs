using System;
using HECSFramework.Core;
using HECSFramework.Documentation;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Components
{
    [Documentation(Doc.Input, "Содержит информацию о настройках ввода Unity.")]
    [Serializable, BluePrint]
    public class InputActionsComponent : BaseComponent
    {
        [SerializeField] private InputActionMap actions;

        public ReadOnlyArray<InputAction> Actions => actions.actions;
    }
}