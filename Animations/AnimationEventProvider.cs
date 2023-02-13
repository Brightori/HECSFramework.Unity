using Commands;
using UnityEngine;

namespace HECSFramework.Unity
{
    public class AnimationEventProvider : MonoBehaviour, IHaveActor
    {
        public Actor Actor { get; set; }

        public void SendAnimationEvent(AnimationEventIdentifier animationEvent)
        {
            Actor.Command(new AnimationEventCommand { Id = animationEvent.Id });
        }

        public void SendStateAnimationEvent(AnimationStateEventBluePrint animationStateEventInfo)
        {
            Actor.Command(new EventStateAnimationCommand { AnimationId = animationStateEventInfo.AnimationEvent.Id, StateId = animationStateEventInfo.AnimatorStateIdentifier.Id });
        }

        public void SendMultiAnimationEvent(MultiAnimationEventBluePrint multiAnimationEvent)
        {
            foreach (var ae in multiAnimationEvent.AnimationEvents)
            {
                Actor.Command(new AnimationEventCommand { Id = ae.Id });
            }

            foreach (var astate in multiAnimationEvent.AnimationStateEvents)
            {
                Actor.Command(new EventStateAnimationCommand { AnimationId = astate.AnimationEvent.Id, StateId = astate.AnimatorStateIdentifier.Id });
            }
        }
    }
}