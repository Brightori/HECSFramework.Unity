using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Camera, "Component with main Camera")]
    public sealed class CameraTagComponent : BaseComponent, IHaveActor 
    {
        public Actor Actor { get; set; }
        public Camera Camera;
        public IdentifierContainer CameraIdentifier;

        public override void Init()
        {
            Actor.TryGetComponent(out Camera, true);
        }
    }
}