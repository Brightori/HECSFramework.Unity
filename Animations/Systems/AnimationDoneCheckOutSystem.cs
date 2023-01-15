using System;
using System.Runtime.CompilerServices;
using Commands;
using Components;
using HECSFramework.Core;
using UnityEngine;

namespace Systems
{
    [Serializable]
    [Documentation(Doc.Animation, Doc.HECS, "This system check when animation done, this system need check animations holder component, and system have double checkup - 1 animation from event, 2 - timer for this animation, its not global system bcz we need check events from animator")]
    public sealed class AnimationDoneCheckOutSystem : BaseSystem, 
        IReactCommand<AnimationEventCommand>, IReactCommand<AnimationDoneCheckOut>, IReactCommand<AnimationCycleCheckout>, IUpdatable
    {
        [Required]
        public AnimationCheckOutsHolderComponent animationCheckOutsHolder;

        private HECSList<AnimationDoneCheckOut> animationDoneCheckOuts = new HECSList<AnimationDoneCheckOut>(8);
        private HECSList<AnimationCycleCheckout> animationCycleDoneCheckOuts = new HECSList<AnimationCycleCheckout>(8);
        private Remover<AnimationDoneCheckOut> animationDoneCheckOutsRemover;

        public void CommandReact(AnimationEventCommand command)
        {
            for (int i = 0; i < animationDoneCheckOuts.Count; i++)
            {
                AnimationDoneCheckOut ac = animationDoneCheckOuts.Data[i];
                if (ac.AnimationEventID == command.Id)
                {
                    Remove(i);
                    return;
                }
            }
        }

        public void CommandReact(AnimationDoneCheckOut command)
        {
            animationDoneCheckOuts.Add(command);
        }

        public void CommandReact(AnimationCycleCheckout command)
        {
            for (int i = 0; i < animationCycleDoneCheckOuts.Count; i++)
            {
                if (animationCycleDoneCheckOuts.Data[i].Equals(command))
                {
                    animationCycleDoneCheckOuts.Data[i] = command;
                    return;
                }
            }

            animationCycleDoneCheckOuts.Add(command);
        }

        public override void InitSystem()
        {
            animationDoneCheckOutsRemover = new Remover<AnimationDoneCheckOut>(animationDoneCheckOuts);
        }

        public void UpdateLocal()
        {
            for (int i = 0; i < animationDoneCheckOuts.Count; i++)
            {
                animationDoneCheckOuts.Data[i].Timing -= Time.deltaTime;

                if (animationDoneCheckOuts.Data[i].Timing <= 0)
                {
                    Remove(i);
                }
            }

            animationDoneCheckOutsRemover.ProcessRemoving();

            CheckCycles();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CheckCycles()
        {
            Span<AnimationCycleCheckout> animationCycleCheckouts = new Span<AnimationCycleCheckout>(animationCycleDoneCheckOuts.Data, 0, animationCycleDoneCheckOuts.Count);

            for (int i = 0; i < animationCycleCheckouts.Length; i++)
            {
                ref var data = ref animationCycleCheckouts[i];

                //todo избавиться тут от дельты
                data.CurrentProgress += Time.deltaTime;

                if (data.CurrentProgress >= data.ActionTime && !data.ActionPassed)
                {
                    data.Action?.Invoke();
                    data.ActionPassed = true;
                }

                if (data.ClipTime <= data.CurrentProgress)
                {
                    data.CycleCount--;
                    data.ResetProgress();

                    if (data.CycleCount <= 0)
                    {
                        data.Complete?.Invoke();
                        animationCycleDoneCheckOuts.Remove(data);
                    }
                    else
                    {
                        data.CycleComplete.Invoke();
                    }
                }
            }
        }

        private void Remove(int index)
        {
            animationDoneCheckOuts.Data[index].CallBack?.Invoke();
            animationDoneCheckOutsRemover.Add(animationDoneCheckOuts.Data[index]);
        }
    }
}