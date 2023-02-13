using System;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.GameLogic, Doc.Actor, Doc.HECS,  "This system update transform component from actor's transform")]
    public sealed class UpdateTranformFromActorSystem : BaseSystem, IHaveActor, IPriorityUpdatable
    {
        public Actor Actor { get; set; }
        public int Priority { get; } = -100;

        [Required]
        public TransformComponent transformComponent;
        
        private Transform transform;

        public override void InitSystem()
        {
            Actor.TryGetComponent(out transform);

            transformComponent.SetPosition(transform.position);
            transformComponent.SetRotation(transform.rotation);
        }

        public void PriorityUpdateLocal()
        {
            transformComponent.SetPosition(transform.position);
            transformComponent.SetRotation(transform.rotation);
        }
    }
}