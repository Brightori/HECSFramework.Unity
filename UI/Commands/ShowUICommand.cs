using System;
using HECSFramework.Core;

namespace Commands
{
    public struct ShowUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
        public Action<Entity> OnUILoad;
        public bool MultyView;

        public int AdditionalCanvasID;
        public int UIGroup;
        public bool ClearStack;
    }
}