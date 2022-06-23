using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Serializable, Documentation(Doc.UI, Doc.Tag, "Это компонент по которому мы определяем принадлежность к UI и храним тут идентификатор UI")]
    public class UITagComponent : BaseComponent
    {
        public UIIdentifier ViewType;
    }
}