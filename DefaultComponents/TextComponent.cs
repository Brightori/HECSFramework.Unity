using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable][Documentation(Doc.Text, "Default component for holding text")]
    public sealed partial class TextComponent : BaseComponent
    {
        public string LocalizationKey;
        public string DefaultText;
    }
}