using Components;
using UnityEngine;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "UIBluePrint", menuName = "BluePrints/UIBluePrint")]
    public class UIBluePrint : ScriptableObject
    {
        public UIActorReference UIActor;
        public UIIdentifier UIType;
        public UIGroupTagComponent Groups;
    }
}