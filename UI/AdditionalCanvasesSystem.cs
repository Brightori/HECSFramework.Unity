using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    public sealed class AdditionalCanvasesSystem : BaseSystem, IGlobalStart, IHaveActor
    {
        public Actor Actor { get; set; }

        [Required]
        public MainCanvasTagComponent MainCanvasTagComponent;

        public void GlobalStart()
        {
            MainCanvasTagComponent.GatherAdditionalCanvases();
        }

        public override void InitSystem()
        {
        }
    }
}
