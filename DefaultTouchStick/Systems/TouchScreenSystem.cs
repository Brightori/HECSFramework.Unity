using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;
using Commands;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.UI, Doc.Input, "Converting Screen Input to ECS Commands")]
    public sealed class TouchScreenSystem : BaseSystem, IHaveActor
    {
        private TouchScreen touchScreen;

        public Actor Actor { get; set; }

        public override void InitSystem()
        {
            Actor.TryGetComponent(out touchScreen, true);

            touchScreen.Drag += DragProcessing;
            touchScreen.Zoom += ZoomProcessing;
        }

        private void ZoomProcessing(float value)
        {
            EntityManager.Command(new ScreenZoomCommand() { Value = value }, -1);
        }

        private void DragProcessing(Vector2 startDrag, Vector2 delta)
        {
            EntityManager.Command(new ScreenDragCommand() { Delta = delta }, -1);
        }

    }
}