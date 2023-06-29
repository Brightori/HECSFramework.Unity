using System;
using HECSFramework.Core;

namespace Commands
{
    [Documentation(Doc.HECS, Doc.UI, "this command response for ")]
    public struct ShowUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
        public Action<Entity> OnUILoad;
        public bool MultyView;
        public bool ShowPrevious;

        public int AdditionalCanvasID;
        public int UIGroup;
        public bool ClearStack;
        public bool Poolable;
    }
}