using HECSFramework.Unity;

namespace HECSFramework.Core
{
    public partial class RegisterService
    {
        partial void RegisterAdditionalSystems(ISystem system)
        {
            if (system is ICustomUpdatable customUpdatable)
                system.Owner.World.RegisterUpdatable(customUpdatable, true);
        }

        partial void UnRegisterAdditionalSystems(ISystem system)
        {
            if (system is ICustomUpdatable customUpdatable)
                system.Owner.World.RegisterUpdatable(customUpdatable, false);
        }
    }
}