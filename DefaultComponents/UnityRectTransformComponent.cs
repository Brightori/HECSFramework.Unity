using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Actor, "This component provide access to unity rect transform, if this entity not actor, this component remove self")]
    public sealed class UnityRectTransformComponent : BaseComponent, IHaveActor, IInitable
    {
        public Actor Actor { get; set; }

        [ReadOnly]
        public RectTransform RectTransform;

        public void Init()
        {
            if (Actor != null)
                RectTransform = Actor.GameObject.GetComponent<RectTransform>();
            else
                Actor.Entity.RemoveComponent(this);
        }
    }
}
