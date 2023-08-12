using Unity.IL2CPP.CompilerServices;

namespace HECSFramework.Core
{
    [Il2CppSetOption(Option.NullChecks, false)]
    [Il2CppSetOption(Option.ArrayBoundsChecks, false)]
    [Il2CppSetOption(Option.DivideByZeroChecks, false)]
    public abstract partial class BaseComponent
    {

    }
}