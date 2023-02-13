using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Components;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    public sealed class AdditionalCanvasesSystem : BaseSystem, IGlobalStart, IHaveActor
    {
        public IActor Actor { get; set; }

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
