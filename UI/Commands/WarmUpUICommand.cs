using HECSFramework.Core;

namespace Commands
{
    public struct WarmUpUICommand : ICommand, IGlobalCommand
    {
        public int UIViewType;
    }
}