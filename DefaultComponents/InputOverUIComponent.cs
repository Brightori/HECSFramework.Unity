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
    public sealed partial class InputOverUIComponent : BaseComponent, IWorldSingleComponent 
    {
        [SerializeField]
        private InputIdentifier inputIdentifier;

        [NonSerialized]
        public List<RaycastResult> RaycastResults = new List<RaycastResult>(32);

        private bool isOverUI;
        private int frameCount;
        private PointerEventData pointerEventData;

        private InputAction inputAction;

        public bool InputOverUI(Vector2 screenPos)
        {
            if (Time.frameCount == frameCount)
                return isOverUI;

            pointerEventData.position = screenPos;
            RaycastAll(pointerEventData, RaycastResults);
            isOverUI = RaycastResults.Count > 0;
            frameCount = Time.frameCount;
            return isOverUI;
        }

        public bool InputOverUI()
        {
            if (Time.frameCount == frameCount)
                return isOverUI;

            pointerEventData.position = inputAction.ReadValue<Vector2>();
            RaycastAll(pointerEventData, RaycastResults);
            isOverUI = RaycastResults.Count > 0;
            frameCount = Time.frameCount;
            return isOverUI;
        }

        private void RaycastAll(PointerEventData eventData, List<RaycastResult> raycastResults)
        {
            raycastResults.Clear();
            var modules = RaycasterManager.GetRaycasters();
            var modulesCount = modules.Count;
            for (int i = 0; i < modulesCount; ++i)
            {
                var module = modules[i];
                if (module == null || !module.IsActive())
                    continue;

                module.Raycast(eventData, raycastResults);
            }
        }

        public override void Init()
        {
            pointerEventData = new PointerEventData(EventSystem.current);

            if (inputIdentifier != null) 
            { 
                Owner.GetComponent<Components.InputActionsComponent>().TryGetInputAction(inputIdentifier.name, out inputAction);
            }
        }
    }
}