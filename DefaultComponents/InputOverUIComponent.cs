using System;
using HECSFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Input, "this component holds bool  - on this moment we over ui or not")]
    public sealed partial class InputOverUIComponent : BaseComponent, IWorldSingleComponent
    {
        private bool isOverUI;
        private int frameCount;

        public bool IsOverUI
        {
            get
            {
                if (Time.frameCount == frameCount)
                    return isOverUI;

                frameCount = Time.frameCount;

                if (Touchscreen.current != null && Touchscreen.current.touches.Count > 0)
                {
                    foreach (var t in Touchscreen.current.touches)
                    {
                        if (EventSystem.current.IsPointerOverGameObject(t.touchId.ReadValue()))
                        {
                            isOverUI = true;
                            return true;
                        }
                    }
                }

                if (EventSystem.current.IsPointerOverGameObject())
                {
                    isOverUI = true;
                    return true;
                }

                isOverUI = false;
                return false;
            }
        }
    }
}