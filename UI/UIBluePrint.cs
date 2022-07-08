using Components;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace HECSFramework.Unity
{
    [CreateAssetMenu(fileName = "UIBluePrint", menuName = "BluePrints/UIBluePrint")]
    public class UIBluePrint : ScriptableObject
    {
        public UIActorReference UIActor;
        public AssetReference Container;
        public UIIdentifier UIType;
        public UIGroupTagComponent Groups;
    }
}