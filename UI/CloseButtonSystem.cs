using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.UI, Doc.HECS, "this is default item for process close buttons ")]
    public sealed class CloseButtonSystem : BaseSystem, IAfterEntityInit 
    {
        private static readonly int Close = IndexGenerator.GenerateIndex("Close"); 

        [Required]
        public UIAccessProviderComponent UIAccessProviderComponent;

        public void AfterEntityInit()
        {
            UIAccessProviderComponent.Get.GetButton(Close).onClick.AddListener(CloseReact);
        }

        public override void InitSystem()
        {
            
        }

        private void CloseReact()
        {
            Owner.Command(new HideUICommand());
        }
    }
}