using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Documentation(Doc.HECS, Doc.Visual, "this system helps to process after view logic on actors without separated view ")]
    public sealed class ActorAfterViewSystem : BaseSystem, IAfterEntityInit, IHaveActor
    {
        public Actor Actor { get; set; }

        public void AfterEntityInit()
        {
            AfterViewService.ProcessAfterView(Owner, Actor.gameObject);
        }

        public override void InitSystem()
        {
        }
    }
}