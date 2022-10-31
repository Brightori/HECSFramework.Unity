using Commands;
using HECSFramework.Core;
using HECSFramework.Unity;
using System;
using UnityEngine;

namespace Components
{
    [Serializable]
    [RequiredAtContainer(typeof(Systems.AnimationDoneCheckOutSystem))]
    [Documentation(Doc.HECS, Doc.Animation, "This component holds needed animation info checkouts")]
    public sealed class AnimationCheckOutsHolderComponent : BaseComponent, IInitable, IValidate
    {
        [SerializeField] private AnimationDoneCheckOutInfo[] animationDoneCheckOutInfos = new AnimationDoneCheckOutInfo[0];

        public void Init()
        {
            foreach (var ac in animationDoneCheckOutInfos)
                ac.FillTiming();
        }

        /// <summary>
        /// this method return animation checkout info, this method usable for strategis or some logic
        /// </summary>
        /// <param name="id"></param>
        /// <param name="animationDoneCheckOutInfo"></param>
        /// <returns></returns>
        public bool TryGetCheckoutInfo(int id, out AnimationDoneCheckOutInfo animationDoneCheckOutInfo)
        {
            foreach (var ac in animationDoneCheckOutInfos)
            {
                if (ac.AnimationEventID.Id == id)
                {
                    animationDoneCheckOutInfo = ac;
                    return true;
                }
            }

            animationDoneCheckOutInfo = default;
            return false;
        }

        /// <summary>
        /// this method return special struct|command with callback, this command for wait animation done system
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="animationDoneCheckOut"></param>
        /// <returns></returns>
        public bool TryGetCheckout(int id, Action callback, out AnimationDoneCheckOut animationDoneCheckOut)
        {
            foreach (var ac in animationDoneCheckOutInfos)
            {
                if (ac.AnimationEventID.Id == id)
                {
                    animationDoneCheckOut = new AnimationDoneCheckOut
                    {
                        AnimationEventID = id,
                        CallBack = callback,
                        Timing = ac.Timing,
                    };

                    return true;
                }
            }

            animationDoneCheckOut = default;
            return false;
        }

        /// <summary>
        /// this method return special struct|command with callback, this command for wait animation done system
        /// </summary>
        /// <param name="id"></param>
        /// <param name="callback"></param>
        /// <param name="animationDoneCheckOut"></param>
        /// <returns></returns>
        public bool TryGetCheckoutCycle(int id, int cycleCount, Action actionCallback, Action cycleComplete, Action completeCallback,  out AnimationCycleCheckout animationCycleCheckout)
        {
            foreach (var ac in animationDoneCheckOutInfos)
            {
                if (ac.AnimationEventID.Id == id)
                {
                    animationCycleCheckout = new AnimationCycleCheckout
                        (
                            Owner.GUID,
                            id = ac.AnimationEventID.Id,
                            ac.Timing,
                            ac.ClipLenght,
                            cycleCount,
                            actionCallback,
                            completeCallback,
                            cycleComplete
                        );
                    return true;
                }
            }

            animationCycleCheckout = default;
            return false;
        }

        public bool IsValid()
        {
            for (int i = 0; i < animationDoneCheckOutInfos.Length; i++)
            {
                animationDoneCheckOutInfos[i].FillTiming();
            }

            return true;
        }
    }
}