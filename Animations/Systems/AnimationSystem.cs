using Commands;
using Components;
using HECSFramework.Core;
using System;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Animation, "System accept commands and set values in AnimationStateComponent by index")]
    public sealed class AnimationSystem : BaseViewSystem, IAnimationSystem
    {
        [Required] public AnimatorStateComponent AnimatorStateComponent;

        public override void InitSystem()
        {
        }

        public void CommandReact(BoolAnimationCommand command)
        {
            if (isReady)
                AnimatorStateComponent.Animator.SetBool(command.Index, command.Value);
        }
        public void CommandReact(TriggerAnimationCommand command)
        {
            if (isReady)
                AnimatorStateComponent.Animator.SetTrigger(command.Index);
        }

        public void CommandReact(FloatAnimationCommand command)
        {
            if (isReady)
                AnimatorStateComponent.Animator.SetFloat(command.Index, command.Value);
        }

        public void CommandReact(IntAnimationCommand command)
        {
            if (isReady)
                AnimatorStateComponent.Animator.SetInteger(command.Index, command.Value);
        }

        protected override void InitAfterViewLocal()
        {
        }

        protected override void ResetLocal()
        {
        }
    }

    public interface IAnimationSystem : ISystem,
        IReactCommand<BoolAnimationCommand>,
        IReactCommand<FloatAnimationCommand>,
        IReactCommand<IntAnimationCommand>,
        IReactCommand<TriggerAnimationCommand>
    { }
}