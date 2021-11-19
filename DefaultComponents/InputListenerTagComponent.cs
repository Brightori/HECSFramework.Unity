using HECSFramework.Core;
using System;

namespace Components
{
    [Documentation("Input", "Глобальная система рассылает ввод всем, кто имеет данный тэг")]
    [Serializable]
    public partial class InputListenerTagComponent : BaseComponent
    {
    }
}