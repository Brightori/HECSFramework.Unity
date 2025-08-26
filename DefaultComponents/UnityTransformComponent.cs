using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Actor, "This component provide access to unity transform, if this entity not actor, this component remove self")]
    public sealed class UnityTransformComponent : BaseComponent, IHaveActor, IDisposable, IHavePosition
    {
        public Actor Actor { get; set; }

        public Vector3 GetPosition => Transform.position;

        [NonSerialized]
        public Transform Transform;

        public override void Init()
        {
            if (Actor != null)
                Transform = Actor.GameObject.GetComponent<Transform>();
            else
                Actor.Entity.RemoveComponent(this);
        }

        public void SetYAxisValue(float value)
        {
            var pos = Transform.position;
            pos.y = value;
            Transform.position = pos;
        }

        public void Dispose()
        {
            Actor = null;
            Transform = null;
        }
    }
}