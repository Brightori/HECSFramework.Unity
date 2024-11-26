using System;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable][Documentation(Doc.Visual, Doc.Abilities, "this system look for init after view systems on abilities, after spawn view")]
    public sealed class InitAbilitiesAfterSpawnViewSystem : BaseSystem , IAfterEntityInit, IInitAfterView
    {
        [Required]
        public AbilitiesHolderComponent AbilitiesHolderComponent;

        public override void InitSystem()
        {
        }

        public void AfterEntityInit()
        {
            if (!Owner.ContainsMask<ViewReferenceGameObjectComponent>())
                InitAbilitiesAfterView();
        }

        public void InitAfterView()
        {
            InitAbilitiesAfterView();
        }

        public void Reset()
        {
            ResetView();
        }

        private void InitAbilitiesAfterView()
        {
            foreach (var a in AbilitiesHolderComponent.Abilities)
            {
                using var initAfterViewComponents = a.GetComponentsOfTypePooled<IInitAfterView>();

                for (int i = 0; i< initAfterViewComponents.Count; i++)
                {
                    initAfterViewComponents.Items[i].InitAfterView();
                }

                foreach (var s in a.Systems)
                {
                    if (s is IInitAfterView initAfterView)
                    {
                        initAfterView.InitAfterView();
                    }
                }
            }
        }

        public void ResetView()
        {
            foreach (var a in AbilitiesHolderComponent.Abilities)
            {
                using var initAfterViewComponents = a.GetComponentsOfTypePooled<IInitAfterView>();

                for (int i = 0; i < initAfterViewComponents.Count; i++)
                {
                    initAfterViewComponents.Items[i].Reset();
                }

                foreach (var s in a.Systems)
                {
                    if (s is IInitAfterView initAfterView)
                    {
                        initAfterView.Reset();
                    }
                }
            }
        }
    }
}