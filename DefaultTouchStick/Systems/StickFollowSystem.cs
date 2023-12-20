using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation(Doc.UI, "Система которая отвечает за перемещение экранного стика")]
    public sealed class StickFollowSystem : BaseSystem, IStickFollowSystem, IAfterEntityInit
    {
        [UsedImplicitly] private const float EditorSpeedMod = 0.1f;

        [Required] private RadiusComponent radiusComponent;
        private Vector2 lastCursorPos;
        private StickWidget stick;
        private StickInputSystem stickInputSystem;

        public Actor Actor { get; set; }

        public override void InitSystem()
        {
           
        }

        public void UpdateLocal()
        {
            // Стики залипают, если в InputSystem приходит эксепшен. Сбрасываем их вручную.
            if (Touchscreen.current != null && Touchscreen.current.touches.Count == 0 && lastCursorPos != Vector2.zero)
            {
                ProcessPointerUp(Vector2.zero);
                return;
            }

            if (stickInputSystem.IsPinched)
            {
                ProcessPointerUp(Vector2.zero);
                return;
            }

            if (lastCursorPos == Vector2.zero) return;

            var deltaUnclamped = lastCursorPos - stick.Center;
            var delta = Vector2.ClampMagnitude(deltaUnclamped, radiusComponent.Radius);
            stick.Rect.anchoredPosition = delta;
            var direction = deltaUnclamped - delta;
            if (direction == Vector2.zero) return;

            direction = direction.Normalized();
            stick.UpdatePosition(direction);
        }

//         private float CalculateSpeed()
//         {
//             var speed = speedComponent.MovementSpeed;
// #if UNITY_EDITOR
//             speed *= EditorSpeedMod;
// #endif
//             return speed;
//         }

        public void ProcessDrag(Vector2 position)
        {
            lastCursorPos = position;
            stick.Delta = Vector2.ClampMagnitude((position - stick.Center) / radiusComponent.Radius, 1);
        }

        public void ProcessPointerDown(Vector2 position)
        {
            lastCursorPos = position;
            stick.SetPosition(position);
        }

        public void ProcessPointerUp(Vector2 position)
        {
            lastCursorPos = Vector2.zero;
            stick.Rect.anchoredPosition = Vector2.zero;
            stick.RestorePosition();
        }

        public void AfterEntityInit()
        {
            stick = Actor.GameObject.GetComponentInChildren<StickWidget>();
            Owner.TryGetSystem(out stickInputSystem);
        }
    }

    public interface IStickFollowSystem : ISystem, IHaveActor, IUpdatable
    {
        void ProcessPointerDown(Vector2 position);
        void ProcessPointerUp(Vector2 position);
        void ProcessDrag(Vector2 position);
    }
}