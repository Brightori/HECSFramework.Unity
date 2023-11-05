using System;
using Components;
using HECSFramework.Core;
using UnityEngine.EventSystems;

namespace Systems
{
    [Serializable][Documentation(Doc.HECS, Doc.Input, "this system update InputOverUIComponent")]
    public sealed class InputOverUISystem : BaseSystem, IPriorityUpdatable 
    {
        [Required]
        public InputOverUIComponent InputOverUIComponent;

        public int Priority { get; } = -50;

        public override void InitSystem()
        {
        }

        public void PriorityUpdateLocal()
        {
            InputOverUIComponent.InputOverUI = EventSystem.current.IsPointerOverGameObject();
        }
    }
}