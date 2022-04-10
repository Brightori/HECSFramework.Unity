using HECSFramework.Core;
using Sirenix.OdinInspector;

namespace HECSFramework.Unity
{
    public abstract class ModifierBluePrint<T> : ModifierBluePrintBase where T : IModifier, new()
    {
        [ShowInInspector] private T modifier = new T();
        
        public override IModifier GetModifier()
        {
            return modifier;
        }
    }
}