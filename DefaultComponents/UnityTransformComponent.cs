using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Actor, "This component provide access to unity transform, if this entity not actor, this component remove self")]
    public sealed class UnityTransformComponent : BaseComponent, IHaveActor, IInitable
    {
        public Actor Actor { get; set; }
        
        [ReadOnly]
        public Transform Transform;

        public void Init()
        {
            if (Actor != null)
                Transform = Actor.GameObject.GetComponent<Transform>();
            else
                Actor.Entity.RemoveComponent(this);
        }
    }
}