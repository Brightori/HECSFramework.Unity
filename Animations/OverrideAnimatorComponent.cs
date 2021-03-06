using System;
using HECSFramework.Core;
using HECSFramework.Unity;
using UnityEngine;

namespace Components
{
    [Serializable]
    [Documentation(Doc.HECS, Doc.Animation, "This component override animations, but u should run  SetupOverrideAnimator() manualy, because in some cases we have view with animator later then actor ")]
    public sealed class OverrideAnimatorComponent : BaseComponent, IInitable, IHaveActor
    {
        [SerializeField] private OverrideAnimatonClip[] overrideAnimatonClips = new OverrideAnimatonClip[0];

        private Animator animator;
        private AnimatorOverrideController animatorOverrideController;
        private AnimatorHelper animatorHelper;

        public IActor Actor { get; set; }

        public void Init()
        {
        }

        public void SetupOverrideAnimator()
        {
            Actor.TryGetComponent(out animator, true);

            animatorHelper = AnimatorManager.GetAnimatorHelper(animator.runtimeAnimatorController.name);

            animatorOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = animatorOverrideController;

            OverrideClips();
        }

        public void OverrideClips()
        {
            foreach (var clipOverride in overrideAnimatonClips)
            {
                animatorHelper.SetOverride(animatorOverrideController, clipOverride.AnimatorStateIdentifier.Id, clipOverride.AnimationClip);
            }
        }
    }
}