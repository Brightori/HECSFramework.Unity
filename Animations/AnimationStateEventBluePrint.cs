using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "AnimationStateEventBluePrint", menuName = "BluePrints/Animation State Event")]
    public class AnimationStateEventBluePrint : ScriptableObject
    {
        public AnimationEventIdentifier AnimationEvent;
        public AnimatorStateIdentifier AnimatorStateIdentifier;
    }
}