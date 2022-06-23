using System;
using HECSFramework.Core;
using HECSFramework.Unity;

namespace Components
{
    [Serializable, Documentation(Doc.UI, Doc.Tag, "��� ��������� �� �������� �� ���������� �������������� � UI � ������ ��� ������������� UI")]
    public class UITagComponent : BaseComponent
    {
        public UIIdentifier ViewType;
    }
}