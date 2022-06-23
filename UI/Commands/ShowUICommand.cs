using System;
using HECSFramework.Core;

namespace Commands
{
    public struct ShowUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
        public Action<IEntity> OnUILoad;
        public bool MultyView;
    }
}