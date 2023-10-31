using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Actor, "This component provide access to unity rect transform, if this entity not actor, this component remove self")]
    public sealed class UnityRectTransformComponent : BaseComponent, IHaveActor, IInitable, IDisposable
    {
        public Actor Actor { get; set; }

        private RectTransform rectTransform;
        public RectTransform RectTransform => rectTransform;

        public void Init()
        {
            if (Actor != null)
                rectTransform = Actor.GameObject.GetComponent<RectTransform>();
            else
                Actor.Entity.RemoveComponent(this);
        }

        public void Dispose()
        {
            Actor = null;
            rectTransform = null;
        }
    }
}
