using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    public sealed partial class AbilitiesHolderComponent : BaseComponent
    {
        public EntityContainer[] AbilitiesContainers = new EntityContainer[0];
    }
}