using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Serializable][Documentation(Doc.Actor, Doc.HECS, "this system looks after all childs actorsand init them with container")]
    public sealed class InitChildActorsSystem : BaseSystem, IHaveActor 
    {
        public Actor Actor { get; set; }

        public override void InitSystem()
        {
            var actorInChilds = Actor.GetComponentsInChildren<Actor>();

            foreach (var child in actorInChilds)
            {
                if (child == Actor)
                    continue;

                if (child.IsInited)
                    continue;

                child.InitWithContainer();
            }
        }
    }
}