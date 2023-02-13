using HECSFramework.Core;


namespace HECSFramework.Unity
{
    public interface IHaveActor : INotCore
    {
        Actor Actor { get; set; }
    }
}

namespace HECSFramework.Core
{
    public interface IInitAferView
    {
        void InitAferView();
    }
}