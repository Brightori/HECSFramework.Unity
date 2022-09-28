using System;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Serializable]
    public struct OverrideAnimatonClip
    {
        public AnimatorStateIdentifier AnimatorStateIdentifier;
        public AnimationClip AnimationClip;
    }
}