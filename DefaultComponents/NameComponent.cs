using System;
using HECSFramework.Core;

namespace Components
{
    [Serializable]
    [Documentation(Doc.Text, Doc.Name, "Default component for holding name of unit or quest, etc")]
    public sealed partial class NameComponent : BaseComponent
    {
        public string LocalizationKey;
        public string DefaultName;
    }
}