using System.Collections.Generic;
using HECSFramework.Serialize;
using UnityEngine;

namespace HECSFramework.Unity
{
    public static partial class AnimatorManager
    {
        private static Dictionary<string, AnimatorHelper> animhelpers = new Dictionary<string, AnimatorHelper>();
        private static Dictionary<string, AnimatorStateResolver> animStateProviders = new Dictionary<string, AnimatorStateResolver>(8);

        public  static AnimatorHelper GetAnimatorHelper(string animatorName)
        {
            if (animhelpers.TryGetValue(animatorName, out var helper))
                   return helper;
            else
                throw new System.Exception("we dont have animator helper for "+animatorName);
        }

        public static AnimatorState GetAnimatorState(string animatorName)
        {
            if (animStateProviders.TryGetValue(animatorName, out var helper))
            {
                var newState = new AnimatorState();
                newState.Load(ref helper);
                return newState;
            }
            else
                throw new System.Exception($"Doesn't have needed provider for Animator {animatorName}, probably u should run codogen");
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