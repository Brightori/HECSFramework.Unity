using HECSFramework.Core;
using Sirenix.OdinInspector;

namespace HECSFramework.Unity
{
    public abstract class ModifierBluePrint<T> : ModifierBluePrintBase where T : IModifier, new()
    {
        [UnityEngine.SerializeField]
        [LabelText("Modifier")]
        private T modifier = new T();

        private void OnEnable()
        {
            modifier.ModifierID = Id;
        }

        public override IModifier GetModifier()
        {
            return modifier;
        }

        public U GetModifierWithCast<U>() where U: IModifier
        {
            if (modifier is U needed)
                return needed;

            return default;
        }
    }
}