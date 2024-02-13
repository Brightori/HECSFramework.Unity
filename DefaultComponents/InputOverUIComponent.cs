using System;
using System.Collections.Generic;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Input, "this component holds bool  - on this moment we over ui or not")]
    public sealed partial class InputOverUIComponent : BaseComponent, IWorldSingleComponent, IInitable
    {
        [SerializeField]
        private InputIdentifier inputIdentifier;

        [NonSerialized]
        public List<RaycastResult> raycastResults = new List<RaycastResult>(32);

        private bool isOverUI;
        private int frameCount;
        private PointerEventData pointerEventData;

        private InputAction inputAction;

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

        public bool InputOverUI()
        {
            if (Time.frameCount == frameCount)
                return isOverUI;

            pointerEventData.position = inputAction.ReadValue<Vector2>();
            EventSystem.current.RaycastAll(pointerEventData, raycastResults);
            isOverUI = raycastResults.Count > 0;
            frameCount = Time.frameCount;
            return isOverUI;
        }

        public void Init()
        {
            pointerEventData = new PointerEventData(EventSystem.current);

            if (inputIdentifier != null) 
            { 
                Owner.GetComponent<Components.InputActionsComponent>().TryGetInputAction(inputIdentifier.name, out inputAction);
            }
        }
    }
}