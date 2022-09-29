using System;
using System.Collections;
using System.Linq;
using HECSFramework.Core;
using HECSFramework.Core.Helpers;
using HECSFramework.Unity;
using HECSFramework.Unity.Helpers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Components
{
    public sealed partial class ScenarioAnimationComponent : BaseComponent, IValidate
    {
        public bool IsValid()
        {
            foreach (var s in ScenarioAnimations)
            {
                foreach (var clips in s.AnimationSteps)
                {
                    var clip =  ReflectionHelpers.GetPrivateFieldValue<AnimationClip>(clips, "animationClip");

                    if (clip == null)
                    {
                        Debug.LogError("we dont have animation clip here");
                        return false;
                    }
                }
            }

            return true;
        }
    }

    public partial struct ScenarioAnimation
    {
        [ShowInInspector]
        [LabelText("Scenario Index")]
        [ValueDropdown(nameof(GetDrawers))]
        [PropertyOrder(-1)]
        public int ScenarioIndexDraw { get => ScenarioIndex; set => ScenarioIndex = value; }

        public IEnumerable GetDrawers()
        {
            var needed = new SOProvider<ScenarioAnimationIdentifier>().GetCollection().ToList();

            var dropDown = new ValueDropdownList<int>();

            foreach (var n in needed)
            {
                dropDown.Add(n.name, n.Id);
            }

            return dropDown;
        }
    }

    public partial struct AnimationHECSInfo
    {
        [ShowInInspector]
        [LabelText("Animation Event")]
        [OnValueChanged(nameof(SetupLenght))]
        [ValueDropdown(nameof(GetDrawers))]
        private int AnimationEventDraw { get => AnimationEvent; set => AnimationEvent = value; }

        public IEnumerable GetDrawers()
        {
            var needed = new SOProvider<AnimationEventIdentifier>().GetCollection().ToList();

            var dropDown = new ValueDropdownList<int>();

            foreach (var n in needed)
            {
                dropDown.Add(n.name, n.Id);
            }

            return dropDown;
        }

        [SerializeField] 
        [OnValueChanged(nameof(SetupLenght))]
        private AnimationClip animationClip;

        private void SetupLenght()
        {
            if (animationClip == null)
                return;

            if (AnimationEvent == 0)
                AnimationLenght = animationClip.length;
            else
            {
                foreach (var e in animationClip.events)
                {
                    if (e.objectReferenceParameter is AnimationEventIdentifier eventIdentifier && eventIdentifier.Id == AnimationEvent)
                    {
                        AnimationLenght = e.time;
                        return;
                    }
                }

                AnimationLenght = animationClip.length;
            }
        }
    }
}
