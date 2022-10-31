using System;
using Commands;
using Components;
using HECSFramework.Core;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Animation, "System accept commands and set values in AnimationStateComponent by index")]
    public sealed class AnimationSystem : BaseSystem,IAnimationSystem
    {
        [Required] public AnimatorStateComponent AnimatorStateComponent;

        public override void InitSystem()
        {
        }

        public void CommandReact(BoolAnimationCommand command)
        {
            AnimatorStateComponent.Animator.SetBool(command.Index, command.Value);
        }
        public void CommandReact(TriggerAnimationCommand command)
        {
            AnimatorStateComponent.Animator.SetTrigger(command.Index);
        }

        public void CommandReact(FloatAnimationCommand command)
        {
            AnimatorStateComponent.Animator.SetFloat(command.Index, command.Value);
        }

        public void CommandReact(IntAnimationCommand command)
        {
            AnimatorStateComponent.Animator.SetInteger(command.Index, command.Value);
        }

        public void CommandReact(ViewReadyCommand command)
        {
            AnimatorStateComponent.SetupAnimatorState();
        }
    }

    public interface IAnimationSystem : ISystem,
          IReactCommand<BoolAnimationCommand>,
        IReactCommand<FloatAnimationCommand>,
        IReactCommand<IntAnimationCommand>,
        IReactCommand<TriggerAnimationCommand>,
        IReactCommand<ViewReadyCommand>
    { }
}