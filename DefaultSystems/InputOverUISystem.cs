using System;
using System.Collections.Generic;
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

        private PointerEventData pointerEventData;
        private List<RaycastResult> raycastResults = new List<RaycastResult>(3);

        public int Priority { get; } = -50;

        public override void InitSystem()
        {
            pointerEventData  = new PointerEventData(EventSystem.current);
        }

        public void PriorityUpdateLocal()
        {
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            InputOverUIComponent.InputOverUI = raycastResults.Count > 0;
        }
    }
}