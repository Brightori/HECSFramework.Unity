using System;
using HECSFramework.Core;

namespace Commands
{
    [Serializable]
    [Documentation(Doc.Animation, Doc.HECS, "This is for cases when we need checkout animation through many cycles, not only one cycle")]
    public struct AnimationCycleCheckout : ICommand
    {
        public Guid Owner;

        public int AnimationEventID;
        public float ActionTime;
        public float ClipTime;
        public float CurrentProgress;
        public bool ActionPassed;

        public int CycleCount;

        public Action Action;
        public Action CycleComplete;
        public Action Complete;

        public AnimationCycleCheckout(Guid owner, int animationEventID, float actionTime, float clipTime, int cycleCount, Action action, Action complete, Action cycleComplete)
        {
            Owner = owner;
            AnimationEventID = animationEventID;
            ActionTime = actionTime;
            ClipTime = clipTime;
            CurrentProgress = 0;
            ActionPassed = false;
            CycleCount = cycleCount;
            Action = action;
            Complete = complete;
            CycleComplete = cycleComplete;
        }

        public override bool Equals(object obj)
        {
            return obj is AnimationCycleCheckout checkout &&
                   Owner.Equals(checkout.Owner) &&
                   AnimationEventID == checkout.AnimationEventID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Owner, AnimationEventID);
        }

        public void ResetProgress()
        {
            CurrentProgress = 0;
            ActionPassed = false;
        }
    }
}