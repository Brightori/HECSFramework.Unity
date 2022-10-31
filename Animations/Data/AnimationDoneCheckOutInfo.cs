using HECSFramework.Core;
using HECSFramework.Unity;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Serializable]
    [Documentation(Doc.Animation, Doc.HECS, "Here we gather info when we need to check if animation done, we save here time and id event from animation clip")]
    public partial struct AnimationDoneCheckOutInfo
    {
        //if animation event wont rise up, we force rise him by timing, but we need timing to be little more then timing of animation event
        public const float ForceTimingOffset = 1.1f;

        [OnValueChanged("FillTiming")]
        public AnimationClip AnimationClip;
        [OnValueChanged("FillTiming")]
        public AnimationEventIdentifier AnimationEventID;

        [ReadOnly]
        [Header("This value should be calculated from animation clip")]
        public float Timing;


        [ReadOnly]
        [Header("lenght at seconds of animation clip")]
        public float ClipLenght;

        [Button("Force check")]
        public void FillTiming()
        {
            ClipLenght = AnimationClip.length;

            if (AnimationClip == null || AnimationEventID == null)
            {
                HECSDebug.LogError("U need fill AnimationDoneCheckOutInfo properly");
                return;
            }

            foreach (var e in AnimationClip.events)
            {
                if (e.objectReferenceParameter == AnimationEventID)
                {
                    Timing = e.time * ForceTimingOffset;
                    return;
                }
            }
        }
    }
}