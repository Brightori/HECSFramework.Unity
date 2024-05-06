using System;
using Components;
using HECSFramework.Core;
using Helpers;

namespace Systems
{
    [Serializable]
    [Feature(Doc.OnAnimatorMoveUpdate)]
    [Documentation(Doc.Animation, Doc.HECS, Doc.OnAnimatorMoveUpdate, "this is main support system, provide injections of IUpdateOnAnimatorMove, to " + nameof(OnAnimatorMoveUpdateProviderMonoComponent))]
    public sealed class InjectAnimatorMoveUpdateSystem : BaseViewSystem
    {
        public override void InitSystem()
        {
        }

        protected override void InitAfterViewLocal()
        {
            SetupUpdates();
        }

        protected override void ResetLocal()
        {
            ClearUpdatables();
            SetupUpdates();
        }

        private void SetupUpdates()
        {
            if (Owner.TryGetComponent(out ViewReadyTagComponent viewReadyTagComponent))
            {

                using (var systems = HECSPooledArray<IUpdateOnAnimatorMove>.GetArray(Owner.Systems.Count))
                {
                    foreach (var s in Owner.Systems) 
                    { 
                        if (s is IUpdateOnAnimatorMove needed)
                        {
                            systems.Add(needed);
                        }
                    }

                    if(systems.Count > 0) 
                    {
                        var onAnimatorUpdate = viewReadyTagComponent.View.GetOrAddMonoComponent<OnAnimatorMoveUpdateProviderMonoComponent>();
                        Owner.GetOrAddComponent<InjectMoveAnimatorUpdateContextComponent>().OnAnimatorMoveUpdateProviderMonoComponent = onAnimatorUpdate;
                        var newArray = new IUpdateOnAnimatorMove[systems.Count];
                        
                        Array.Copy(systems.Items, newArray, systems.Count);
                        onAnimatorUpdate.OnAnimatorMoveUpdatables = newArray;
                    }
                }
            }
        }

        public override void Dispose()
        {
            ClearUpdatables();
        }

        private void ClearUpdatables()
        {
            if (Owner.TryGetComponent(out InjectMoveAnimatorUpdateContextComponent injectMoveAnimatorUpdateContextComponent))
            {
                if (injectMoveAnimatorUpdateContextComponent.OnAnimatorMoveUpdateProviderMonoComponent != null)
                {
                    injectMoveAnimatorUpdateContextComponent.OnAnimatorMoveUpdateProviderMonoComponent.OnAnimatorMoveUpdatables = Array.Empty<IUpdateOnAnimatorMove>();
                }
            }
        }
    }
}