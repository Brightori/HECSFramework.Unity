using System;
using HECSFramework.Unity;
using HECSFramework.Core;
using UnityEngine;
using Components;

namespace Systems
{
    [Serializable][Documentation(Doc.GameLogic, Doc.HECS, Doc.Actor, "System for update actor position from transform component")]
    public sealed class UpdateActorByTranformSystem : BaseSystem, IHaveActor
    {
        public Actor Actor { get; set; }
        public int Priority { get; } = -100;

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