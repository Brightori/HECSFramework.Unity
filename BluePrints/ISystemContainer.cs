using HECSFramework.Core;

namespace HECSFramework.Unity
{
    public interface ISystemContainer
    {
        ISystem GetSystem { get; }
    }
}