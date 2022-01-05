namespace HECSFramework.Core
{
    public partial class RegisterService
    {
        partial void RegisterAdditionalSystems(ISystem system)
        {
            // функционал подписки/отписки дополнительных систем реализован в GlobalUpdateSystem
        }

        partial void UnRegisterAdditionalSystems(ISystem system)
        {
            // функционал подписки/отписки дополнительных систем реализован в GlobalUpdateSystem
        }
    }
}