using HECSFramework.Core;

namespace HECSFrameWork.Systems
{
    public interface ISystemContainer
    {
        ISystem GetSystem { get; }
    }
}