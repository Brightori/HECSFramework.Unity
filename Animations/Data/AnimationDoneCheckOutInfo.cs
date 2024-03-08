using System;
using HECSFramework.Core;
using Sirenix.OdinInspector;
using UnityEngine;

namespace HECSFramework.Unity
{
    [Serializable]
    [Documentation(Doc.Animation, Doc.HECS, "Here we gather info when we need to check if animation done, we save here time and id event from animation clip")]
    public partial struct AnimationDoneCheckOutInfo
    {
        [OnValueChanged("FillTiming")]
        public AnimationClip AnimationClip;
        [OnValueChanged("FillTiming")]
        public AnimationEventIdentifier AnimationEventID;

        [ReadOnly]
        [Header("This value should be calculated from animation clip")]
        public float Timing;

        [ReadOnly]
        [Header("This value should be calculated from animation clip")]
        public float TimingNormalized;

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
                    Timing = e.time;
                    TimingNormalized = e.time/ClipLenght;
                    return;
                }
            }
        }
    }
}