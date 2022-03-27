using System.Collections.Generic;
using UnityEngine;

namespace HECSFramework.Unity
{
    public static partial class AnimatorManager
    {
        private static Dictionary<string, AnimatorHelper> animhelpers = new Dictionary<string, AnimatorHelper>();
        

        public  static AnimatorHelper GetAnimatorHelper(string animatorName)
        {
            if (animhelpers.TryGetValue(animatorName, out var helper))
                   return helper;
            else
                throw new System.Exception("we dont have animator helper for "+animatorName);
        }
    }

    public class AnimatorHelper
    {
        private Dictionary<int, string> stateToAnimation;

        public AnimatorHelper(Dictionary<int, string> stateToAnimation)
        {
            this.stateToAnimation = stateToAnimation;
        }

        public void SetOverride(AnimatorOverrideController animatorToOverride, int stateIndex, AnimationClip animationClip)
        {
            if (stateToAnimation.TryGetValue(stateIndex, out var clipName))
            {
                animatorToOverride[clipName] = animationClip;
            }
        }
    }
}