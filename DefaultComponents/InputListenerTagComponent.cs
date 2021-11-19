using System;
using HECSFramework.Core;
using HECSFramework.Network;

namespace Components
{
    [Documentation("Input", "Глобальная система рассылает ввод всем, кто имеет данный тэг")]
    [Serializable]
    public partial class InputListenerTagComponent : BaseComponent, INotReplicable, IClientSide
    {
    }
}