using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "MultiAnimationEventBluePrint", menuName = "BluePrints/MultiAnimation Event")]
    public class MultiAnimationEventBluePrint : ScriptableObject
    {
        public AnimationEventIdentifier[] AnimationEvents = new AnimationEventIdentifier[0];
        public AnimationStateEventBluePrint[] AnimationStateEvents = new AnimationStateEventBluePrint[0];
    }
}