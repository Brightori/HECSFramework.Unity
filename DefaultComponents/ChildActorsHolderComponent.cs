using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Visual, "this component holds child actors for external access")]
    public sealed class ChildActorsHolderComponent : BaseComponent, IHaveActor, IInitAfterView
    {
        [ReadOnly]
        public Actor[] ChildActors = new Actor[0];
        public Actor Actor { get; set; }

        public override void Init()
        {
            Setup();
        }

        private void Setup()
        {
            Actor.TryGetComponents(out ChildActors);
        }


        public void InitAfterView()
        {
            Setup();
        }

        public void Reset()
        {
        }
    }
}