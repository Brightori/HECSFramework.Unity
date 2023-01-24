using System;
using HECSFramework.Core;

namespace Commands
{
    public struct ShowUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
        public Action<Entity> OnUILoad;
        public bool MultyView;
    }

    public struct ShowUIOnAdditionalCommand: IGlobalCommand
    {
        public int AdditionalCanvasID;
        public int UIViewType;
        public Action<Entity> OnUILoad;
        public bool MultyView;
    }
}