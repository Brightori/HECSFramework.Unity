using HECSFramework.Core;

namespace HECSFramework.Unity
{
    public interface IComponentContainer
    {
        IComponent GetHECSComponent { get; }
    }
}