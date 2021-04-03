using HECSFramework.Core;
using HECSFramework.Unity;
using System;

namespace Components
{
    [Serializable, BluePrint]
    public partial class ItemTagComponent : BaseComponent, IItemTagComponent
    {
       
    }

    public interface IItemTagComponent : IComponent
    {
    }
}