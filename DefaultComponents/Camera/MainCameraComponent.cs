using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Camera, "Component with main Camera")]
    public sealed class MainCameraComponent : BaseComponent, IHaveActor, IWorldSingleComponent
    {
        public Actor Actor { get; set; }
        public Camera Camera;

        public override void Init()
        {
            Actor.TryGetComponent(out Camera, true);
        }
    }
}