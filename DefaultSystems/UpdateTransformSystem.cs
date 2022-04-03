using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;

namespace Systems
{
	[Serializable][Documentation(Doc.GameLogic, Doc.Actor, "System for update transform component")]
    public sealed class UpdateTransformSystem : BaseSystem, IHaveActor 
    {
        private ConcurrencyList<Vector3> m_Vertices = new ConcurrencyList<Vector3>();

        public IActor Actor { get; set; }

        [Required]
        private TransformComponent transformComponent;
        private Transform transform;

        public override void InitSystem()
        {
            Actor.TryGetComponent(out transform);

            transformComponent.SetPosition(transform.position);
            transformComponent.SetRotation(transform.rotation);
        }

        public void UpdateLateLocal()
        {
            if (transformComponent.IsDirty)
            {
                transform.position = transformComponent.Position;
                transform.rotation = transformComponent.Rotation;
                transformComponent.IsDirty = false;
            }
        }
    }
}