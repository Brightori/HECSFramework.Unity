using System;
using System.Collections.Generic;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Input, "this component holds bool  - on this moment we over ui or not")]
    public sealed partial class InputOverUIComponent : BaseComponent, IWorldSingleComponent, IInitable
    {
        private bool isOverUI;
        private int frameCount;
        private List<RaycastResult> raycastResults = new List<RaycastResult>(32);
        private PointerEventData pointerEventData;

        public bool InputOverUI(Vector2 screenPos)
        {
            if (Time.frameCount == frameCount)
                return isOverUI;

            pointerEventData.position = screenPos;
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            isOverUI = raycastResults.Count > 0;
            frameCount = Time.frameCount;
            return isOverUI;
        }

        public void Init()
        {
            pointerEventData = new PointerEventData(EventSystem.current);
        }
    }
}