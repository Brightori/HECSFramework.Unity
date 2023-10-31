using HECSFramework.Core;
using System;

namespace Commands
{
    [Documentation(Doc.UI, "������� ������� �������� �� ����� ��� ������� ������ UI")]
    public struct UIGroupCommand : IGlobalCommand
    {
        public bool Show;
        public int UIGroup;
        public Action OnLoadUI;
    }
}