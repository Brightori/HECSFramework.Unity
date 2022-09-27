using System;
using Components;
using HECSFramework.Core;

namespace Commands
{
    [Serializable]
    [Documentation(Doc.Animation, Doc.HECS, "We send this command when we need to know when animation will be done, we take this command from AnimationCheckOutsHolderComponent")]
    public struct AnimationDoneCheckOut : ICommand
    {
        public int AnimationEventID;
        public float Timing;
        public Action CallBack;

        public override bool Equals(object obj)
        {
            return obj is AnimationDoneCheckOut @out &&
                   AnimationEventID == @out.AnimationEventID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AnimationEventID);
        }
    }
}