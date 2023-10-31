using HECSFramework.Core;
using System;

namespace Commands
{
    [Documentation(Doc.UI, "Команда которая отвечает за показ или скрытие группы UI")]
    public struct UIGroupCommand : IGlobalCommand
    {
        public bool Show;
        public int UIGroup;
        public Action OnLoadUI;
    }
}