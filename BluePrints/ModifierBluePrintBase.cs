using HECSFramework.Core;

namespace HECSFramework.Unity
{
    public abstract class ModifierBluePrintBase : ModifierIdentifier
    {
        public abstract IModifier GetModifier();
    }
}
