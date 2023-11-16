using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Systems
{
    [Serializable, BluePrint]
    [Documentation("Stick", "Система которая перехватывает нажатия на область действия экранного стика")]
    public class StickInputSystem : BaseSystem, IStickInputSystem, IAfterEntityInit
    {
        private StickPanelWidget stickPanel;
        private IStickFollowSystem followSystem;

        public bool IsPinched { get; private set; }

        public Actor Actor { get; set; }

        public override void InitSystem()
        {
           
        }

        public override void Dispose()
        {
            base.Dispose();
            if (stickPanel == null)
                return;

            stickPanel.Dragged -= followSystem.ProcessDrag;
            stickPanel.PointerDown -= followSystem.ProcessPointerDown;
            stickPanel.PointerUp -= followSystem.ProcessPointerUp;
        }

        public void AfterEntityInit()
        {
            Owner.TryGetSystem(out followSystem);
            if (!Actor.TryGetComponent(out stickPanel, true)) return;

            stickPanel.Dragged += followSystem.ProcessDrag;
            stickPanel.PointerDown += followSystem.ProcessPointerDown;
            stickPanel.PointerUp += followSystem.ProcessPointerUp;
        }
    }

    public interface IStickInputSystem : ISystem, IHaveActor
    { }
}